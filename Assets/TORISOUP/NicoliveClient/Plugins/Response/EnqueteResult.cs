using System;
using System.Linq;

namespace TORISOUP.NicoliveClient.Response
{
    /// <summary>
    /// アンケート実行結果
    /// </summary>
    public struct EnqueteResult
    {
        /// <summary>
        /// アンケートタイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 結果要素
        /// </summary>
        public EnqueteItem[] Items { get; private set; }

        public EnqueteResult(string title, EnqueteItem[] items) : this()
        {
            Title = title;
            Items = items;
        }
    }

    /// <summary>
    /// アンケート結果の要素
    /// </summary>
    public struct EnqueteItem
    {
        /// <summary>
        /// 設問
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 回答率
        /// </summary>
        public float Rate { get; private set; }

        public EnqueteItem(string name, float rate) : this()
        {
            Name = name;
            Rate = rate;
        }
    }

    #region dto

    [Serializable]
    internal struct EnqueteResultDto
    {
        public string title;
        public EnqueteItemDto[] items;

        public EnqueteResult ToEnqueteResult()
        {
            return new EnqueteResult(title, items.Select(x => x.ToEnqueteItem()).ToArray());
        }
    }

    [Serializable]
    internal struct EnqueteItemDto
    {
        public string name;
        public float rate;

        public EnqueteItem ToEnqueteItem()
        {
            return new EnqueteItem(name, rate);
        }
    }

    #endregion
}
