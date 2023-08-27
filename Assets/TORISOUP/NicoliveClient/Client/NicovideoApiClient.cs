using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
        public static IObservable<MylistInfo> GetNicovideoInfoFromMylist(ulong mylistId)
        {
            return Observable.FromCoroutine<MylistInfo>(
                observer => GetNicovideoInfoFromMylistCoroutine(observer, mylistId, null)).Kick();
        }

        public static IObservable<MylistInfo> GetNicovideoInfoFromMylist(ulong mylistId, NiconicoUser user)
        {
            return Observable.FromCoroutine<MylistInfo>(
                observer => GetNicovideoInfoFromMylistCoroutine(observer, mylistId, user.UserSession)).Kick();
        }

        private static IEnumerator GetNicovideoInfoFromMylistCoroutine(
            IObserver<MylistInfo> observer, ulong mylistId, string userSession)
        {
            var url = string.Format("https://www.nicovideo.jp/mylist/{0}?rss=2.0", mylistId);

            using (var www = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(userSession)) www.SetRequestHeader("Cookie", "user_session=" + userSession);

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

#if UNITY_2017_1_OR_NEWER
                if (www.isHttpError || www.isNetworkError)
#else
                if (www.isError)
#endif
                {
                    observer.OnError(new NicovideoApiClientException(www.downloadHandler.text));
                    yield break;
                }

                try
                {
                    var titleRegex = new Regex(@"マイリスト (\w+)‐ニコニコ動画");
                    var uriRegex = new Regex(@"src=""(https?://[\w/:%#\$&\?\(\)~\.=\+\-]+)""");
                    var viodeIdRegex = new Regex(@"(sm\d+)");

                    var xml = www.downloadHandler.text;

                    // XMLをパースして動画情報を取得
                    var xdoc = XDocument.Parse(xml);
                    var rss = xdoc.Element("rss");
                    var channel = rss.Element("channel");
                    var t = channel.Element("title").Value;
                    var mylistTitle = titleRegex.Match(t).Groups[1].Value;

                    var videos = channel.Elements("item").Select(i =>
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
                    }).Where(x => !string.IsNullOrEmpty(x.VideoId)).ToArray();

                    observer.OnNext(new MylistInfo(mylistTitle, videos));
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                    observer.OnError(ex);
                    yield break;
                }
            }
        }
    }

    public class NicovideoApiClientException : Exception
    {
        public NicovideoApiClientException(string message) : base(message)
        {
        }
    }
}