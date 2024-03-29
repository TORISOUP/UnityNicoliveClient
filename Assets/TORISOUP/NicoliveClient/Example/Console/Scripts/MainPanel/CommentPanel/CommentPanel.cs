﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using R3.Triggers;
using TORISOUP.NicoliveClient.Client;
using TORISOUP.NicoliveClient.Comment;
using TORISOUP.NicoliveClient.Response;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.CommentPanel
{
    /// <summary>
    /// 接続可能な部屋に繋いでコメントを受信するサンプル
    /// 接続・切断のたびにクライアントを全部作り直している
    /// </summary>
    public class CommentPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _connectButton;

        [SerializeField] private Button _disconnectButton;

        [SerializeField] private Text _roomCountText;

        [SerializeField] private Button _getRoomButton;

        /// <summary>
        /// 受信したコメント情報
        /// </summary>
        public Observable<Chat> OnCommentReceived => _onCommentReceived;

        //接続状態
        private readonly ReactiveProperty<bool> _isConnected = new();

        //接続しているObservableを切断する時に必要
        private IDisposable _receiveDisposable;

        //全クランアントから配送されたコメント情報を全てこのSubjectに集約する
        private readonly Subject<Chat> _onCommentReceived = new();

        private IEnumerable<NicoliveCommentClient> _commentClients;

        private readonly ReactiveProperty<CommentServerInfo[]> _commentServerInfos = new();

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            // 接続先情報を取得する
            _getRoomButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        var info = await _manager.NicoliveApiClient.GetProgramInfoAsync(c);
                        _commentServerInfos.OnNext(info.Rooms.Select(r => r.CommentServerInfo).ToArray());
                    }
                    catch (Exception e)when (e is not OperationCanceledException)
                    {
                        _commentServerInfos.Value = Array.Empty<CommentServerInfo>();
                    }
                }, cancellationToken: ct)
                .Forget();

            //部屋数が更新されたらUIへ反映する
            _commentServerInfos
                .Where(x => x != null)
                .Select(x => x.Length)
                .Subscribe(x =>
                {
                    //接続していない かつ 部屋数が1以上のときのみ接続できる
                    _connectButton.interactable = !_isConnected.Value && x > 0;
                    _roomCountText.text = $"現在の部屋数:{x}";
                })
                .AddTo(ct);

            //接続状態が更新されたらボタンに反映する
            _isConnected.Subscribe(x =>
                {
                    _connectButton.interactable = !x && _commentServerInfos.Value.Length > 0;
                    _disconnectButton.interactable = x;
                })
                .AddTo(ct);

            //コメントサーバに接続
            _connectButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Connect();
                    _isConnected.Value = true;
                })
                .AddTo(ct);

            //コメントサーバから切断
            _disconnectButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Disconnect();
                    _isConnected.Value = false;
                })
                .AddTo(ct);

            //GameObjectが破棄されたら切断する
            this.OnDestroyAsObservable()
                .Subscribe(_ => Disconnect())
                .AddTo(ct);
        }

        /// <summary>
        /// 接続可能な全ての部屋に接続する
        /// </summary>
        private void Connect()
        {
            Disconnect();

            if (_commentServerInfos.Value == null) return;

            //各部屋ごとのクライアント作成
            _commentClients = _commentServerInfos.Value
                .Select(x => new NicoliveCommentClient(x, _manager.CurrentUser.UserId))
                .ToArray();

            if (_commentClients == null) return;
            
            //全部屋の情報をまとめて1つのObservableにする
            _receiveDisposable =
                Observable
                    .Merge(_commentClients.Select(x => x.OnMessageAsObservable))
                    .Subscribe(x => _onCommentReceived.OnNext(x)); // Mergeした結果を1つのSubjectに流し込む

            //接続開始
            foreach (var c in _commentClients)
            {
                //過去10件取得
                c.Connect(resFrom: 10);
            }
        }

        /// <summary>
        /// 切断する
        /// </summary>
        private void Disconnect()
        {
            //先にObservableを停止
            _receiveDisposable?.Dispose();

            //各クライアントを破棄
            if (_commentClients != null)
            {
                foreach (var c in _commentClients)
                {
                    c.Dispose();
                }

                _commentClients = null;
            }
        }
    }
}