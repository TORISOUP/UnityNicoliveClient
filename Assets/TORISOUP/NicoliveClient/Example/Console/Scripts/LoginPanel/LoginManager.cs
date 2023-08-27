using TORISOUP.NicoliveClient.Client;
using UniRx;
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

        private ReactiveProperty<bool> _isLoggedIn = new ReactiveProperty<bool>();

        private ReactiveProperty<string> _errorMessage = new ReactiveProperty<string>();


        /// <summary>
        /// ログイン状態であるか
        /// </summary>
        public IReadOnlyReactiveProperty<bool> IsLoggedIn
        {
            get { return _isLoggedIn; }
        }

        /// <summary>
        /// ログイン失敗時のメッセージ
        /// </summary>
        public IReadOnlyReactiveProperty<string> ErrorMessage
        {
            get { return _errorMessage; }
        }


        /// <summary>
        /// ログイン処理を実行する
        /// </summary>
        public void Login(string mail, string pass)
        {
            _errorMessage.Value = "";

            NiconicoUserClient.LoginAsync(mail, pass)
                .Subscribe(x =>
                {
                    CurrentUser = x;
                    _isLoggedIn.Value = true;
                }, ex =>
                {
                    _errorMessage.Value = ex.Message;
                    _isLoggedIn.Value = false;
                });
        }
    }
}
