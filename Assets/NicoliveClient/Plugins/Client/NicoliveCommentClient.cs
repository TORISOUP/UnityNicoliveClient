using System;
using NicoliveClient;
using UniRx;
using WebSocketSharp;

namespace NicoliveClient
{
    /// <summary>
    /// コメントサーバに接続してコメントを受信するクライアント
    /// </summary>
    public class NicoliveCommentClient : IDisposable
    {
        public Uri WebSocketUri { get; private set; }
        public string RoomName { get; private set; }
        public int RoomId { get; private set; }

        public IObservable<string> OnMessageAsObservable
        {
            get { return _onMessageConnectableObservable; }
        }

        private WebSocket _ws;

        private IConnectableObservable<string> _onMessageConnectableObservable;
        private IDisposable _onMessageDisposable;

        #region コンストラクタ
        public NicoliveCommentClient(Uri webSocketUri, string roomName, int roomId)
        {
            WebSocketUri = webSocketUri;
            RoomName = roomName;
            RoomId = roomId;
            Initialize();
        }

        public NicoliveCommentClient(Uri webSocketUri)
        {
            WebSocketUri = webSocketUri;
            RoomName = "";
            RoomId = 0; //default
            Initialize();
        }

        public NicoliveCommentClient(Room room)
        {
            WebSocketUri = room.Uri;
            RoomName = room.Name;
            RoomId = room.Id;
            Initialize();
        }

        private void Initialize()
        {
            _ws = new WebSocket(WebSocketUri.AbsoluteUri);
            _onMessageConnectableObservable =
                Observable.FromEventPattern<EventHandler<MessageEventArgs>, MessageEventArgs>(
                    h => h.Invoke,
                    h => _ws.OnMessage += h,
                    h => _ws.OnMessage -= h).Select(x => x.EventArgs.Data).Publish();
        }

        #endregion

        /// <summary>
        /// コメントサーバに接続する
        /// </summary>
        public void Connect()
        {
            _onMessageDisposable = _onMessageConnectableObservable.Connect();
            _ws.Connect();
        }

        /// <summary>
        /// コメントサーバから切断する
        /// </summary>
        public void Disconnect()
        {
            if (_onMessageDisposable != null) _onMessageDisposable.Dispose();
            _ws.Close();
        }

        public void Dispose()
        {
            if (_ws != null)
            {
                _ws.Close();
                _ws = null;
            }

            if (_onMessageDisposable != null)
            {
                _onMessageDisposable.Dispose();
            }
        }
    }
}
