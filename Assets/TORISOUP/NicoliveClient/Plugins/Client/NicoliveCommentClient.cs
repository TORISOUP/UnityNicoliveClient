﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TORISOUP.NicoliveClient.Comment;
using TORISOUP.NicoliveClient.Response;
using UniRx;
using UnityEngine;
using WebSocketSharp;

namespace TORISOUP.NicoliveClient.Client
{
    /// <summary>
    /// コメントサーバに接続してコメントを受信するクライアント
    /// </summary>
    public class NicoliveCommentClient : IDisposable
    {
        /// <summary>
        /// 接続しているWebSocketURI
        /// </summary>
        public Uri WebSocketUri { get; private set; }

        /// <summary>
        /// 接続している部屋名
        /// </summary>
        public string RoomName { get; private set; }

        /// <summary>
        /// 部屋のID取得
        /// </summary>
        public int RoomId { get; private set; }

        /// <summary>
        /// スレッドID
        /// </summary>
        public string ThreadId { get; private set; }

        private IObservable<Chat> _onMessageAsObservable;
        private readonly object _lockObject = new object();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 受信したコメントオブジェクトを通知する
        /// </summary>
        public IObservable<Chat> OnMessageAsObservable
        {
            get
            {
                lock (_lockObject)
                {
                    return !_isDisposed ? _onMessageAsObservable : Observable.Empty<Chat>();
                }
            }
        }

        private bool _isDisposed;
        private readonly string _userId;
        private WebSocket _ws;
        private AsyncSubject<Unit> _disposedEventAsyncSubject;

        #region コンストラクタ

        public NicoliveCommentClient(Room room, string userId)
        {
            WebSocketUri = room.WebSocketUri;
            RoomName = room.Name;
            RoomId = room.Id;
            _userId = userId;
            ThreadId = room.ThreadId;
            Initialize();
        }

        public NicoliveCommentClient(CommentServerInfo commentServerInfo, string userId)
        {
            WebSocketUri = commentServerInfo.WebSocketUri;
            RoomName = "";
            RoomId = 0;
            _userId = userId;
            ThreadId = commentServerInfo.Thread;
            Initialize();
        }

        private void Initialize()
        {
            _ws = new WebSocket(WebSocketUri.AbsoluteUri, "msg.nicovideo.jp#json");

            _disposedEventAsyncSubject = new AsyncSubject<Unit>();
            _onMessageAsObservable =
                Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                        h => (sender, e) => h(e),
                        h => _ws.OnMessage += h,
                        h => _ws.OnMessage -= h)
                    .ObserveOn(Scheduler.ThreadPool) //Jsonのパース処理をThreadPoolで行う
                    .Select(x => JsonUtility.FromJson<CommentDto>(x.Data))
                    .Where(x => x.chat.IsSuccess())
                    .Select(x => x.chat.ToChat(RoomId))
                    .TakeUntil(_disposedEventAsyncSubject)
                    .ObserveOnMainThread() //Unityメインスレッドに戻す
                    .Share();
        }

        #endregion

        /// <summary>
        /// コメントサーバに接続する
        /// </summary>
        /// <param name="resFrom">過去何件分取得するか</param>
        public void Connect(int resFrom)
        {
            Disconnect();
            _cancellationTokenSource = new CancellationTokenSource();

            if (resFrom < 0) resFrom = 0;

            ConnectAsync(resFrom, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid ConnectAsync(int resFrom, CancellationToken ct)
        {
            if (_ws == null) return;

            await UniTask.SwitchToThreadPool();

            _ws.Connect();

            //初期化のJson
            _ws.Send(
                "[{\"ping\":{\"content\":\"rs:0\"}},{\"ping\":{\"content\":\"ps:0\"}},"
                + "{\"thread\":{\"thread\":\"" + ThreadId + "\",\"version\":\"20061206\",\"fork\":0,"
                + "\"user_id\":\"" + _userId + "\",\"res_from\":-" + resFrom +
                ",\"with_global\":1,\"scores\":1,\"nicoru\":0}},"
                + "{\"ping\":{\"content\":\"pf:0\"}},{\"ping\":{\"content\":\"rf:0\"}}]"
            );

            // pingを定期的に投げる
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken: ct).ConfigureAwait(false);
                if (_ws != null && _ws.IsAlive) _ws.Ping();
            }
        }

        /// <summary>
        /// コメントサーバから切断する
        /// </summary>
        public void Disconnect()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            if (_ws != null && _ws.IsAlive)
            {
                _ws.CloseAsync();
            }
        }

        /// <summary>
        /// クライアントを破棄する
        /// </summary>
        public void Dispose()
        {
            lock (_lockObject)
            {
                Disconnect();

                if (_disposedEventAsyncSubject != null)
                {
                    _disposedEventAsyncSubject.OnNext(Unit.Default);
                    _disposedEventAsyncSubject.OnCompleted();
                    _disposedEventAsyncSubject.Dispose();
                }

                _ws = null;
                _isDisposed = true;
            }
        }
    }
}