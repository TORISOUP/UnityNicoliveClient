using System;

namespace NicoliveClient
{
    public struct StatisticsResult
    {
        /// <summary>
        /// 累計来場者数
        /// </summary>
        public int WatchCount { get; private set; }

        /// <summary>
        /// 累計コメント数（運営コメントを含まず）
        /// </summary>
        public int CommentCount { get; private set; }

        public StatisticsResult(int watchCount, int commentCount) : this()
        {
            WatchCount = watchCount;
            CommentCount = commentCount;
        }
    }

    [Serializable]
    internal struct StatisticsResultDto
    {
        public int watchCount;
        public int commentCount;

        public StatisticsResult ToStatisticsResult()
        {
            return new StatisticsResult(watchCount, commentCount);
        }
    }
}
