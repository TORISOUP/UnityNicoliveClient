using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TORISOUP.NicoliveClient.Client;
using UnityEngine;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.LoginPanel
{
    /// <summary>
    /// ログイン状態の管理
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        /// <summary>
        /// ログイン中のユーザ情報
        /// </summary>
        public NiconicoUser CurrentUser { get; private set; }

        private readonly ReactiveProperty<bool> _isLoggedIn = new();


        /// <summary>
        /// ログイン状態であるか
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsLoggedIn => _isLoggedIn;

        /// <summary>
        /// ログイン処理を実行する
        /// </summary>
        public async UniTask LoginAsync(string mail, string pass, CancellationToken ct)
        {
            try
            {
                CurrentUser = await NiconicoUserClient.LoginAsync(mail, pass, ct);
                _isLoggedIn.Value = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                CurrentUser = default;
                _isLoggedIn.Value = false;
            }
        }

        public void SaveUser(NiconicoUser niconicoUser)
        {
            PlayerPrefs.SetString("UserId", niconicoUser.UserId);
            PlayerPrefs.SetString("UserSession", niconicoUser.UserSession);
        }
        
        public void LoadUser()
        {
            var userId = PlayerPrefs.GetString("UserId");
            var userSession = PlayerPrefs.GetString("UserSession");
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userSession))
            {
                _isLoggedIn.Value = false;
                return;
            }
            CurrentUser = new NiconicoUser(userId, userSession);
            _isLoggedIn.Value = true;
        }
        
        public void Logout()
        {
            PlayerPrefs.DeleteKey("UserId");
            PlayerPrefs.DeleteKey("UserSession");
            CurrentUser = default;
            _isLoggedIn.Value = false;
        }
    }
}