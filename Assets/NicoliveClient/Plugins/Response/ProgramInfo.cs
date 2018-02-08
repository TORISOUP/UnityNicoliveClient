using System;
using System.Diagnostics;
using System.Linq;

namespace NicoliveClient
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
            string[] categories) : this()
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

        public SocialGroup(string name, string id, string type) : this()
        {
            Name = name;
            Id = id;
            Type = type == "community" ? SocialGroupType.Community : SocialGroupType.Channel;
        }
    }

    /// <summary>
    /// コメントサーバの情報
    /// </summary>
    public struct Room
    {
        /// <summary>
        /// 部屋名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 番組における部屋のID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// コメントサーバのURI（WebSocket）
        /// </summary>
        public Uri WebSocketUri { get; private set; }

        /// <summary>
        /// コメントサーバのURI（XmlSocket）
        /// </summary>
        public Uri XmlSocketUri { get; private set; }

        /// <summary>
        /// スレッドID
        /// </summary>
        public string ThreadId { get; private set; }

        public Room(string name, int id, string webSocketUri, string xmlSocketUri,string threadId) : this()
        {
            Name = name;
            Id = id;
            WebSocketUri = new Uri(webSocketUri);
            XmlSocketUri = new Uri(xmlSocketUri);
            ThreadId = threadId;
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
                categories
                );

        }
    }

    [Serializable]
    internal struct SocialGroupDto
    {
        public string name;
        public string id;
        public string type;

        public SocialGroup ToSocialGroup()
        {
            return new SocialGroup(name, id, type);
        }
    }

    [Serializable]
    internal struct RoomDto
    {
        public string name;
        public int id;
        public string webSocketUri;
        public string xmlSocketUri;
        public string threadId;

        public Room ToRoom()
        {
            return new Room(name, id, webSocketUri, xmlSocketUri, threadId);
        }
    }

    #endregion

}
