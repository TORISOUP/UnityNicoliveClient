using System;

namespace TORISOUP.NicoliveClient
{
    public struct NiconicoUser : IEquatable<NiconicoUser>
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// ユーザセッション（APIの実行に必要）
        /// </summary>
        public string UserSession { get; private set; }

        public NiconicoUser(string userId, string userSession) : this()
        {
            UserId = userId;
            UserSession = userSession;
        }

        public bool Equals(NiconicoUser other)
        {
            return UserId == other.UserId && UserSession == other.UserSession;
        }

        public override bool Equals(object obj)
        {
            return obj is NiconicoUser other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, UserSession);
        }
    }
}