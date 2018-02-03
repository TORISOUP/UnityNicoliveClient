using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace NicoliveClient.Example
{
    /// <summary>
    /// 接続可能な部屋に繋いでコメントを受信するサンプル
    /// 接続・切断のたびにクライアントを全部作り直しているのでパフォーマンスは良くない
    /// </summary>
    public class CommentPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Text _roomCountText;

        /// <summary>
        /// 受信したコメント情報
        /// </summary>
        public IObservable<string> OnCommentRecieved
        {
            get { return _onCommentRecieved; }
        }

        //接続状態
        private ReactiveProperty<bool> _isConnected = new BoolReactiveProperty();

        //接続しているObservableを切断する時に必要
        private IDisposable _reciveDisposable;

        //全クランアントから配送されたコメント情報を全てこのSubjectに集約する
        private Subject<string> _onCommentRecieved = new Subject<string>();

        private IEnumerable<NicoliveCommentClient> _commentClients;

        void Start()
        {
            //部屋数が更新されたらUIへ反映する
            _manager.CurrentRooms
                .ObserveCountChanged()
                .Subscribe(x =>
                {
                    //接続していない かつ 部屋数が1以上のときのみ接続できる
                    _connectButton.interactable = !_isConnected.Value && x > 0;
                    _roomCountText.text = string.Format("現在の部屋数:{0}", x);
                });

            //接続状態が更新されたらボタンに反映する
            _isConnected.Subscribe(x =>
            {
                _connectButton.interactable = !x && _manager.CurrentRooms.Count > 0;
                _disconnectButton.interactable = x;
            });

            //コメントサーバに接続
            _connectButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Connect();
                    _isConnected.Value = true;
                });

            //コメントサーバから切断
            _disconnectButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Disconnect();
                    _isConnected.Value = false;
                });

            //GameObjectが破棄されたら切断する
            this.OnDestroyAsObservable()
                .Subscribe(_ => Disconnect());
        }

        /// <summary>
        /// 接続可能な全ての部屋に接続する
        /// </summary>
        private void Connect()
        {
            //各部屋ごとのクライアント作成
            _commentClients = _manager.CurrentRooms.Values.Select(x => new NicoliveCommentClient(x));

            //全部屋の情報をまとめて1つのObservableにする
            _reciveDisposable =
                _commentClients
                .Select(x => x.OnMessageAsObservable)
                .Merge()
                .TakeUntilDestroy(this) //このGameObjectが破棄されたらOnCompletedを差し込む
                .Multicast(_onCommentRecieved) // Mergeした結果を1つのSubjectに流し込む
                .Connect();

            //接続開始
            foreach (var c in _commentClients)
            {
                c.Connect();
            }
        }

        /// <summary>
        /// 切断する
        /// </summary>
        private void Disconnect()
        {
            //先にObservableを停止
            if (_reciveDisposable != null) _reciveDisposable.Dispose();

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
