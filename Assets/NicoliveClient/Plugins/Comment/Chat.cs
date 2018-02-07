using System;

namespace Assets.NicoliveClient.Plugins.Comment
{
    /// <summary>
    /// コメントブジェクト
    /// </summary>
    public struct Chat
    {
        /// <summary>
        /// ThreadId
        /// </summary>
        public string Thread { get; private set; }

        /// <summary>
        /// コメント基準時刻からの経過時間（センチ秒）
        /// </summary>
        public long Vpos { get; private set; }

        /// <summary>
        /// コメント投稿時刻（UNIXTIME）
        /// </summary>
        public long Date { get; private set; }

        /// <summary>
        /// コマンド欄
        /// </summary>
        public string Mail { get; private set; }

        /// <summary>
        /// ユーザのID（184のときはハッシュ値）
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// ユーザ状態（ビット列）
        /// </summary>
        public int Premium { get; private set; }

        /// <summary>
        /// 184であるか
        /// </summary>
        public bool Anonymity { get; private set; }

        /// <summary>
        /// コメント本文
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// コメント番号
        /// </summary>
        public int No { get; private set; }

        /// <summary>
        /// 部屋のID
        /// </summary>
        public int RoomId { get; private set; }

        public Chat(string thread, long vpos, long date, string mail, string userId, int premium, bool anonymity, string content, int no, int roomId) : this()
        {
            Thread = thread;
            Vpos = vpos;
            Date = date;
            Mail = mail;
            UserId = userId;
            Premium = premium;
            Anonymity = anonymity;
            Content = content;
            No = no;
            RoomId = roomId;
        }

        /// <summary>
        /// プレミアム会員であるか
        /// </summary>
        public bool IsPremium
        {
            get { return (Premium & 1) != 0; }
        }

        /// <summary>
        /// 運営コメント　または /から始まる特殊なメッセージであるか
        /// </summary>
        public bool IsStaff
        {
            get { return (Premium & 2) != 0; }
        }
    }

    [Serializable]
    internal struct ChatDto
    {
        public string thread;
        public long vpos;
        public long date;
        public string mail;
        public string user_id;
        public int premium;
        public int anonymity;
        public string content;
        public int no;

        public bool IsSuccess()
        {
            return !string.IsNullOrEmpty(content);
        }

        public Chat ToChat(int roomId)
        {
            return new Chat(
                thread,
                vpos,
                date,
                mail,
                user_id,
                premium,
                anonymity > 0,
                content,
                no,
                roomId
                );
        }
    }

    [Serializable]
    internal struct CommentDto
    {
        public ChatDto chat;
    }
}
