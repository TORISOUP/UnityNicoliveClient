using System;
using System.Collections;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace TORISOUP.NicoliveClient.Client
{
    /// <summary>
    /// ニコニコのアカウント系APIを実行してユーザ情報を返すクライアント
    /// </summary>
    public static class NiconicoUserClient
    {
        /// <summary>
        /// ニコニコにログインする
        /// </summary>
        /// <param name="mail">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <returns>成功時:NiconicoUser,失敗時:NiconicoLoginException</returns>
        public static IObservable<NiconicoUser> LoginAsync(string mail, string password)
        {
            return Observable.FromCoroutine<NiconicoUser>(o => LoginCoroutine(o, mail, password));
        }

        private static IEnumerator LoginCoroutine(IObserver<NiconicoUser> observer, string mail, string password)
        {
            var url = "https://account.nicovideo.jp/api/v1/login";

            var userSessionRegex = new Regex(@"user_session=user_session_(\w+);");

            var form = new WWWForm();
            form.AddField("mail", mail);
            form.AddField("password", password);

            using (var www = UnityWebRequest.Post(url, form))
            {
                www.redirectLimit = 0;

#if UNITY_2017_2_OR_NEWER
                yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif

                var cookie = www.GetResponseHeader("Set-Cookie");
                var match = userSessionRegex.Match(cookie);

                if (match.Success)
                {
                    var userSession = "user_session_" + match.Groups[1];
                    var userId = match.Groups[1].ToString().Split('_')[0];
                    observer.OnNext(new NiconicoUser(userId, userSession));
                    observer.OnCompleted();
                }
                else
                {
                    observer.OnError(new NiconicoLoginException("ログインに失敗しました"));
                }
            }
        }
    }

    public class NiconicoLoginException : Exception
    {
        public NiconicoLoginException(string message) : base(message)
        {
        }
    }
}
