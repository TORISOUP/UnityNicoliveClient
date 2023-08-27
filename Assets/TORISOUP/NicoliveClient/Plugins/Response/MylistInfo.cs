namespace TORISOUP.NicoliveClient.Response
{
    /// <summary>
    /// ニコニコ動画のマイリスト情報
    /// </summary>
    public struct MylistInfo
    {
        /// <summary>
        /// マイリストのタイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// マイリスト内の動画情報
        /// </summary>
        public VideoInfo[] Videos { get; private set; }

        public MylistInfo(string title, VideoInfo[] videos) : this()
        {
            Title = title;
            Videos = videos;
        }
    }

    /// <summary>
    /// ニコニコ動画の動画情報
    /// </summary>
    public struct VideoInfo
    {
        public string Title { get; private set; }
        public string VideoId { get; private set; }
        public string ThumbnailUri { get; private set; }

        public VideoInfo(string title, string videoId, string thumbnailUri) : this()
        {
            Title = title;
            VideoId = videoId;
            ThumbnailUri = thumbnailUri;
        }

        public override string ToString()
        {
            return string.Format("Title: {0}, VideoId: {1}, ThumbnailUri: {2}", Title, VideoId, ThumbnailUri);
        }
    }
}