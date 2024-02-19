using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.LoginPanel
{
    /// <summary>
    /// ログイン画面のUIとの接合部
    /// </summary>
    public class LoginPresenter : MonoBehaviour
    {
        [SerializeField] private LoginManager _loginManager;

        [SerializeField] private InputField _mail;
        [SerializeField] private InputField _pass;
        [SerializeField] private Button _singIn;
        [SerializeField] private Text _errorMessage;

        [SerializeField] private Button _logoutButton;
        [SerializeField] private Button _loginBySavedUserButton;

        void Start()
        {
            //ログインボタンが押された

            _singIn
                .OnClickAsAsyncEnumerable(this.GetCancellationTokenOnDestroy())
                .ForEachAwaitWithCancellationAsync(
                    async (_, ct) =>
                    {
                        try
                        {
                            _errorMessage.text = "";
                            await _loginManager.LoginAsync(_mail.text, _pass.text, ct);

                            _loginManager.SaveUser(_loginManager.CurrentUser);
                        }
                        catch (Exception e)
                        {
                            _errorMessage.text = e.Message;
                        }
                    },
                    this.GetCancellationTokenOnDestroy())
                .Forget();

            _loginBySavedUserButton
                .OnClickAsAsyncEnumerable(this.GetCancellationTokenOnDestroy())
                .ForEachAsync(
                    _ =>
                    {
                        try
                        {
                            _errorMessage.text = "";
                            _loginManager.LoadUser();
                        }
                        catch (Exception e)
                        {
                            _errorMessage.text = e.Message;
                        }
                    },
                    this.GetCancellationTokenOnDestroy())
                .Forget();
            
            _logoutButton
                .OnClickAsAsyncEnumerable(this.GetCancellationTokenOnDestroy())
                .ForEachAsync(
                    _ =>
                    {
                        _loginManager.Logout();
                    },
                    this.GetCancellationTokenOnDestroy())
                .Forget();
        }
    }
}