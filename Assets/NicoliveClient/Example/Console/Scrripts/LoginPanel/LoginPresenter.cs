using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
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

        void Start()
        {
            //ログインボタンが押された
            _singIn.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(3)) //連打防止
                .Subscribe(_ =>
                {
                    _loginManager.Login(_mail.text, _pass.text);
                });

            //エラーメッセージ表示
            _loginManager.ErrorMessage.SubscribeToText(_errorMessage);

        }

    }
}
