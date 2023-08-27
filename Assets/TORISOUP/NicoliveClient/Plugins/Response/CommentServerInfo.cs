using System;

namespace TORISOUP.NicoliveClient.Response
{
    /// <summary>
    /// コメントサーバ情報
    /// </summary>
    public struct CommentServerInfo
    {
        public string RoomName { get; }
        public Uri WebSocketUri { get; }
        public string Thread { get; }

        public CommentServerInfo(string address, int port, string threadId, string roomName)
        {
            WebSocketUri = new Uri($"ws://{address}:{port}/websocket");
            RoomName = roomName;
            Thread = threadId;
        }

        public CommentServerInfo(string roomName, Uri webSocketUri, string thread)
        {
            RoomName = roomName;
            WebSocketUri = webSocketUri;
            Thread = thread;
        }
    }
}