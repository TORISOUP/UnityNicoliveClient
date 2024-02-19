using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TORISOUP.NicoliveClient.Comment;
using TORISOUP.NicoliveClient.Response;
using UnityEngine;
using WebSocketSharp;
using R3;

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

        private Observable<Chat> _onMessageAsObservable;
        private readonly object _lockObject = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Subject<Unit> _resetSubject = new();

        /// <summary>
        /// 受信したコメントオブジェクトを通知する
        /// </summary>
        public Observable<Chat> OnMessageAsObservable
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

            var ct = _cancellationTokenSource.Token;

            _onMessageAsObservable =
                Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                        h => (sender, e) => h(e),
                        h => _ws.OnMessage += h,
                        h => _ws.OnMessage -= h,
                        ct)
                    .ObserveOnThreadPool()
                    .Select(x => JsonUtility.FromJson<CommentDto>(x.Data))
                    .Where(x => x.chat.IsSuccess())
                    .Select(x => x.chat.ToChat(RoomId))
                    .ObserveOnMainThread() //Unityメインスレッドに戻す
                    .TakeUntil(_resetSubject)
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
                if (_ws is { IsAlive: true }) _ws.Ping();
            }
        }
    
        /// <summary>
        /// コメントサーバから切断し、Observableをリセットする
        /// </summary>
        public void Disconnect()
        {
            if (_ws is { IsAlive: true })
            {
                _resetSubject.OnNext(Unit.Default);
                _ws.Close();
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

                _resetSubject.OnCompleted();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _ws = null;
                _isDisposed = true;
            }
        }
    }
}