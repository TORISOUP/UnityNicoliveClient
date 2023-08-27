using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using TORISOUP.NicoliveClient.Response;
using TORISOUP.NicoliveClient.Utilities;
using UniRx;
using UnityEngine.Networking;

namespace TORISOUP.NicoliveClient.Client
{
    /// <summary>
    /// ニコニコ動画向けAPIClient
    /// </summary>
    public static class NicovideoApiClient
    {
        public static async UniTask<MylistInfo> GetNicovideoInfoFromMylistAsync(ulong mylistId, CancellationToken ct)
        {
            return await _GetNicovideoInfoFromMylistAsync(mylistId, null, ct);
        }

        public static async UniTask<MylistInfo> GetNicovideoInfoFromMylistAsync(ulong mylistId,
            NiconicoUser user,
            CancellationToken ct)
        {
            return await _GetNicovideoInfoFromMylistAsync(mylistId, user.UserSession, ct);
        }

        private static async UniTask<MylistInfo> _GetNicovideoInfoFromMylistAsync(
            ulong mylistId,
            string session,
            CancellationToken ct)
        {
            var url = $"https://www.nicovideo.jp/mylist/{mylistId}?rss=2.0";

            using var uwr = UnityWebRequest.Get(url);
            if (!string.IsNullOrEmpty(session)) uwr.SetRequestHeader("Cookie", "user_session=" + session);


            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new NicovideoApiClientException(uwr.downloadHandler.text, ex);
            }

            var titleRegex = new Regex(@"マイリスト (\w+)‐ニコニコ動画");
            var uriRegex = new Regex(@"src=""(https?://[\w/:%#\$&\?\(\)~\.=\+\-]+)""");
            var viodeIdRegex = new Regex(@"(sm\d+)");

            var xml = uwr.downloadHandler.text;

            // XMLをパースして動画情報を取得
            var xdoc = XDocument.Parse(xml);
            var rss = xdoc.Element("rss");
            var channel = rss.Element("channel");
            var t = channel.Element("title").Value;
            var mylistTitle = titleRegex.Match(t).Groups[1].Value;

            var videos = channel.Elements("item")
                .Select(i =>
                {
                    try
                    {
                        var title = i.Element("title").Value;
                        var link = i.Element("link").Value;
                        var des = i.Element("description").Value;
                        var thumbnailUri = uriRegex.Match(des).Groups[1].Value;
                        var videoId = viodeIdRegex.Match(link).Groups[1].Value;
                        return new VideoInfo(title, videoId, thumbnailUri);
                    }
                    catch
                    {
                        return default(VideoInfo);
                    }
                })
                .Where(x => !string.IsNullOrEmpty(x.VideoId))
                .ToArray();

            return (new MylistInfo(mylistTitle, videos));
        }
    }

    public class NicovideoApiClientException : Exception
    {
        public NicovideoApiClientException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}