namespace NicoliveClient
{
    public struct NiconicoUser
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
    }
}
