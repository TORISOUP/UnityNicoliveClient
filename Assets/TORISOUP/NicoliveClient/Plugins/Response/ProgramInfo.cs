using System;
using System.Linq;

namespace TORISOUP.NicoliveClient.Response
{
    public struct ProgramInfo
    {
        /// <summary>
        /// 番組の属するグループ
        /// </summary>
        public SocialGroup SocialGroup { get; private set; }

        /// <summary>
        /// 部屋情報
        /// </summary>
        public Room[] Rooms { get; private set; }

        /// <summary>
        /// 放送状態
        /// </summary>
        public ProgramStatus Status { get; private set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 概要
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// フォロワー限定放送か
        /// </summary>
        public bool IsMemberOnly { get; private set; }

        /// <summary>
        /// コメントのVpos値の基準時刻
        /// </summary>
        public long VposBaseAt { get; private set; }

        /// <summary>
        /// テスト放送を含まない本番放送開始時刻
        /// </summary>
        public long BeginAt { get; private set; }

        /// <summary>
        /// 終了時刻（放送中は終了予定時刻）
        /// </summary>
        public long EndAt { get; private set; }

        /// <summary>
        /// カテゴリタグ
        /// </summary>
        public string[] Categories { get; private set; }

        /// <summary>
        /// ニコニ広告を許可しているか
        /// </summary>
        public bool IsAdsEnabled { get; private set; }

        /// <summary>
        /// 配信者情報
        /// </summary>
        public Broadcaster Broadcaster { get; private set; }

        /// <summary>
        /// 配信設定
        /// </summary>
        public StreamSetting StreamSetting { get; private set; }

        /// <summary>
        /// モデレータ向けの情報が配信されるViewUri
        /// （視聴者のときはnull）
        /// </summary>
        public string ModeratorViewUri { get; private set; }

        public ProgramInfo(
            SocialGroup socialGroup,
            Room[] rooms,
            string status,
            string title,
            string description,
            bool isMemberOnly,
            long vposBaseAt,
            long beginAt,
            long endAt,
            string[] categories,
            bool isAdsEnabled,
            Broadcaster broadcaster,
            StreamSetting streamSetting,
            string moderatorViewUri
        ) : this()
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
                case "ended":
                    s = ProgramStatus.Ended;
                    break;
            }

            SocialGroup = socialGroup;
            Rooms = rooms;
            Status = s;
            Title = title;
            Description = description;
            IsMemberOnly = isMemberOnly;
            VposBaseAt = vposBaseAt;
            BeginAt = beginAt;
            EndAt = endAt;
            Categories = categories;
            IsAdsEnabled = isAdsEnabled;
            Broadcaster = broadcaster;
            StreamSetting = streamSetting;
            ModeratorViewUri = moderatorViewUri;
        }
    }

    /// <summary>
    /// 番組の属するグループ
    /// </summary>
    public struct SocialGroup
    {
        /// <summary>
        /// コミュニテイ名/チャンネル名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// コミュニテイID/チャンネルID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// コミュニテイであるかチャンネルであるか
        /// </summary>
        public SocialGroupType Type { get; private set; }

        /// <summary>
        /// チャンネル番組時：チャンネルの配信会社名
        /// コミュニテイ番組時：null
        /// </summary>
        public string OwnerName { get; private set; }

        /// <summary>
        /// コミュニテイレベル（コミュニテイ番組の場合のみ有効な数値が入る）
        /// </summary>
        public int CommunityLevel { get; private set; }

        public SocialGroup(string name, string id, string type, string ownerName, int level) : this()
        {
            Name = name;
            Id = id;
            Type = type == "community" ? SocialGroupType.Community : SocialGroupType.Channel;
            OwnerName = ownerName;
            CommunityLevel = level;
        }
    }

    /// <summary>
    /// コメントサーバの情報
    /// </summary>
    public struct Room
    {
        /// <summary>
        /// コメントサーバに接続するための情報
        /// </summary>
        public string ViewUri { get; }

        public Room(string viewUri)
        {
            ViewUri = viewUri;
        }
    }

    /// <summary>
    /// 配信者情報
    /// </summary>
    public struct Broadcaster
    {
        /// <summary>
        /// 配信者のユーザID / チャンネルの場合はチャンネルID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 配信者名
        /// </summary>
        public string Name { get; private set; }

        public Broadcaster(int id, string name) : this()
        {
            Id = id;
            Name = name;
        }
    }

    public struct StreamSetting
    {
        public string MaxQuality { get; }
        public string Orientation { get; }

        public StreamSetting(string maxQuality, string orientation)
        {
            MaxQuality = maxQuality;
            Orientation = orientation;
        }
    }

    public enum ProgramStatus
    {
        Test,
        OnAir,
        Ended,
        Reserved
    }

    public enum SocialGroupType
    {
        Community,
        Channel
    }

    #region DTO

    [Serializable]
    internal struct ProgramInfoDto
    {
        public SocialGroupDto socialGroup;
        public RoomDto[] rooms;
        public string status;
        public string title;
        public string description;
        public bool isMemberOnly;
        public long vposBaseAt;
        public long beginAt;
        public long endAt;
        public string[] categories;
        public bool isAdsEnabled;
        public BroadcasterDto broadcaster;
        public bool isUserNiconicoAdsEnabled;
        public StreamSettingDto streamSetting;
        public string moderatorViewUri;

        public ProgramInfo ToProgramInfo()
        {
            return new ProgramInfo(
                socialGroup.ToSocialGroup(),
                rooms.Select(x => x.ToRoom()).ToArray(),
                status,
                title,
                description,
                isMemberOnly,
                vposBaseAt,
                beginAt,
                endAt,
                categories,
                isAdsEnabled,
                broadcaster.ToBroadcaster(),
                streamSetting.ToStreamSetting(),
                moderatorViewUri
            );
        }
    }

    [Serializable]
    internal struct SocialGroupDto
    {
        public string name;
        public string id;
        public string type;
        public string ownerName;
        public int communityLevel;

        public SocialGroup ToSocialGroup()
        {
            return new SocialGroup(name, id, type, ownerName, communityLevel);
        }
    }

    [Serializable]
    internal struct RoomDto
    {
        public string viewUri;

        public Room ToRoom()
        {
            return new Room(viewUri);
        }
    }

    [Serializable]
    internal struct BroadcasterDto
    {
        public int id;
        public string name;

        public Broadcaster ToBroadcaster()
        {
            return new Broadcaster(id, name);
        }
    }

    [Serializable]
    internal struct StreamSettingDto
    {
        public string maxQuality;
        public string orientation;

        public StreamSetting ToStreamSetting()
        {
            return new StreamSetting(maxQuality, orientation);
        }
    }

    #endregion
}