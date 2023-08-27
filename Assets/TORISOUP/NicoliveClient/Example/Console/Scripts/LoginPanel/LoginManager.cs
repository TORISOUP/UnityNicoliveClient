using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        private readonly ReactiveProperty<bool> _isLoggedIn = new ReactiveProperty<bool>();


        /// <summary>
        /// ログイン状態であるか
        /// </summary>
        public IReadOnlyReactiveProperty<bool> IsLoggedIn => _isLoggedIn;

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
    }
}