using System;

namespace TORISOUP.NicoliveClient.Comment
{
    /// <summary>
    /// コメントブジェクト
    /// </summary>
    public struct Chat : IEquatable<Chat>
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

        /// <summary>
        /// NGスコア
        /// </summary>
        /// <value></value>
        public int Score { get; private set; }

        public Chat(string thread,
            long vpos,
            long date,
            string mail,
            string userId,
            int premium,
            bool anonymity,
            string content,
            int no,
            int roomId,
            int score) : this()
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
            Score = score;
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

        public bool Equals(Chat other)
        {
            return Thread == other.Thread && Vpos == other.Vpos && Date == other.Date && Mail == other.Mail &&
                   UserId == other.UserId && Premium == other.Premium && Anonymity == other.Anonymity &&
                   Content == other.Content && No == other.No && RoomId == other.RoomId && Score == other.Score;
        }

        public override bool Equals(object obj)
        {
            return obj is Chat other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Thread);
            hashCode.Add(Vpos);
            hashCode.Add(Date);
            hashCode.Add(Mail);
            hashCode.Add(UserId);
            hashCode.Add(Premium);
            hashCode.Add(Anonymity);
            hashCode.Add(Content);
            hashCode.Add(No);
            hashCode.Add(RoomId);
            hashCode.Add(Score);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    internal struct ChatDto : IEquatable<ChatDto>
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
        public int score;

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
                roomId,
                score
            );
        }

        public bool Equals(ChatDto other)
        {
            return thread == other.thread && vpos == other.vpos && date == other.date && mail == other.mail &&
                   user_id == other.user_id && premium == other.premium && anonymity == other.anonymity &&
                   content == other.content && no == other.no && score == other.score;
        }

        public override bool Equals(object obj)
        {
            return obj is ChatDto other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(thread);
            hashCode.Add(vpos);
            hashCode.Add(date);
            hashCode.Add(mail);
            hashCode.Add(user_id);
            hashCode.Add(premium);
            hashCode.Add(anonymity);
            hashCode.Add(content);
            hashCode.Add(no);
            hashCode.Add(score);
            return hashCode.ToHashCode();
        }
    }

    [Serializable]
    internal struct CommentDto
    {
        public ChatDto chat;
    }
}