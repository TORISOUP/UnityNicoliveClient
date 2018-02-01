namespace NicoliveClient
{
    public struct NiconicoUser
    {
        /// <summary>
        /// ユーザセッション（APIの実行に必要）
        /// </summary>
        public string UserSession { get; private set; }

        public NiconicoUser(string userSession) : this()
        {
            UserSession = userSession;
        }
    }
}
