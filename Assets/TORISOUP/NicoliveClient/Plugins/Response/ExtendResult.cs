using System;

namespace NicoliveClient
{
    public struct ExtendResult
    {
        /// <summary>
        /// 終了予定時刻
        /// </summary>
        public long EndTime { get; private set; }

        public ExtendResult(long endTime) : this()
        {
            EndTime = endTime;
        }
    }

    [Serializable]
    internal struct ExtendDto
    {
        public long end_time;
    }
}
