using System;

namespace TORISOUP.NicoliveClient.Response
{
    /// <summary>
    /// コメントサーバ情報
    /// </summary>
    public struct CommentServerInfo
    {
        public string RoomName { get; }
        public Uri XmlSocketUri { get; }
        public Uri WebSocketUri { get; }
        public string Thread { get; }

        public CommentServerInfo(string address, int port, string threadId, string roomName)
        {
            XmlSocketUri = new Uri(string.Format("xmlsocket://{0}:{1}", address, port));
            WebSocketUri = new Uri(string.Format("ws://{0}:{1}/websocket", address, port));
            RoomName = roomName;
            Thread = threadId;
        }

        public CommentServerInfo(string roomName, Uri webSocketUri,Uri xmlSocketUri, string thread)
        {
            RoomName = roomName;
            WebSocketUri = webSocketUri;
            XmlSocketUri = xmlSocketUri;
            Thread = thread;
        }
    }
}
