using System;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        /// 2段階認証は未対応
        /// </summary>
        /// <param name="mail">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <returns>成功時:NiconicoUser,失敗時:NiconicoLoginException</returns>
        public static async UniTask<NiconicoUser> LoginAsync(string mail, string password, CancellationToken ct)
        {
            var url = "https://account.nicovideo.jp/api/v1/login";

            var userSessionRegex = new Regex(@"user_session=user_session_(\w+);");

            var form = new WWWForm();
            form.AddField("mail", mail);
            form.AddField("password", password);

            using var uwr = UnityWebRequest.Post(url, form);
            uwr.redirectLimit = 0;

            try
            {
                await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Redirect limit exceededは無視
                if (!ex.Message.Contains("Redirect limit exceeded"))
                {
                    throw;
                }
            }

            var cookie = uwr.GetResponseHeader("Set-Cookie");
            var match = userSessionRegex.Match(cookie);

            if (!match.Success) throw new NiconicoLoginException("ログインに失敗しました");

            var userSession = "user_session_" + match.Groups[1];
            var userId = match.Groups[1].ToString().Split('_')[0];
            return new NiconicoUser(userId, userSession);
        }
    }

    public class NiconicoLoginException : Exception
    {
        public NiconicoLoginException(string message) : base(message)
        {
        }
    }
}