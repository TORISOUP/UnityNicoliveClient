using System;

namespace NicoliveClient
{
    public struct ProgramSchedule
    {
        /// <summary>
        /// 番組ID
        /// </summary>
        public string ProgramId { get; private set; }
        
        /// <summary>
        /// テスト放送開始時刻
        /// </summary>
        public long TestBeginAt { get; private set; }
        
        /// <summary>
        /// 本放送開始予定時刻
        /// </summary>
        public long OnAirBeginAt { get; private set; }
        
        /// <summary>
        /// 本放送終了予定時刻
        /// </summary>
        public long OnAirEndAt { get; private set; }
        
        /// <summary>
        /// コミュニテイであるかチャンネルであるか
        /// </summary>
        public SocialGroupType SocialGroupType { get; private set; }
        
        /// <summary>
        /// コミュニティID/チャンネルID
        /// </summary>
        public string SocialGroupId { get; private set; }
        
        /// <summary>
        /// 番組状態
        /// </summary>
        public ProgramStatus Status { get; private set; }

        public ProgramSchedule(string programId, long testBeginAt, long onAirBeginAt, long onAirEndAt, SocialGroupType socialGroupType, string socialGroupId, ProgramStatus status)
        {
            ProgramId = programId;
            TestBeginAt = testBeginAt;
            OnAirBeginAt = onAirBeginAt;
            OnAirEndAt = onAirEndAt;
            SocialGroupType = socialGroupType;
            SocialGroupId = socialGroupId;
            Status = status;
        }
    }

    #region DTO

    [Serializable]
    internal struct ProgramScheduleDto
    {
        public string nicoliveProgramId;
        public long testBeginAt;
        public long onAirBeginAt;
        public long onAirEndAt;
        public string socialGroupId;
        public string status;

        public ProgramSchedule ToProgramSchedule()
        {
            ProgramStatus s;
            switch (status)
            {
                case "test":
                    s = ProgramStatus.Test;
                    break;
                case "onAir":
                    s = ProgramStatus.OnAir;
                    break;
                case "reserved":
                    s = ProgramStatus.Reserved;
                    break;
                default:
                case "end": //終了番組はこのAPIでは返ってこないはずではある
                    s = ProgramStatus.Ended;
                    break;
            }

            var socialGroupType = socialGroupId.Substring(0, 2) == "co"
                ? SocialGroupType.Community
                : SocialGroupType.Channel;

            return new ProgramSchedule(
                nicoliveProgramId,
                testBeginAt,
                onAirBeginAt,
                onAirEndAt,
                socialGroupType,
                socialGroupId,
                s
            );
        }
    }

    #endregion
}
