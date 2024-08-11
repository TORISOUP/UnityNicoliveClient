using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using TORISOUP.NicoliveClient.Client;
using TORISOUP.NicoliveClient.Example.Console.Scripts.LoginPanel;
using TORISOUP.NicoliveClient.Response;
using R3;
using UnityEngine;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts
{
    public class NicoliveSampleManager : MonoBehaviour
    {
        [SerializeField] private LoginManager _loginManager;
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _blockPanel;
        
        private Regex _lvRegex = new Regex(@"^lv\d+$");

        public NicoliveApiClient NicoliveApiClient
        {
            get { return _nicoliveApiClient; }
        }

        /// <summary>
        /// 現在の部屋情報
        /// </summary>
        public Dictionary<string, Room> CurrentRooms => _currentRooms;

        /// <summary>
        /// 現在のユーザ情報
        /// </summary>
        public NiconicoUser CurrentUser
        {
            get { return _loginManager.CurrentUser; }
        }

        private readonly Dictionary<string, Room> _currentRooms = new();
        private NicoliveApiClient _nicoliveApiClient;
        private readonly ReactiveProperty<string> _currentProgramId = new ReactiveProperty<string>("");


        /// <summary>
        /// 現在の対象番組ID
        /// </summary>
        public ReadOnlyReactiveProperty<string> CurrentProgramId
        {
            get { return _currentProgramId; }
        }

        private ReadOnlyReactiveProperty<bool> _isSetProgramId;

        /// <summary>
        /// 有効な番組IDが設定されているか
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsSetProgramId
        {
            get
            {
                return _isSetProgramId ??=
                    CurrentProgramId.Select(x => !string.IsNullOrEmpty(x))
                        .ToReadOnlyReactiveProperty();
            }
        }

        private void Start()
        {
            _loginManager.IsLoggedIn
                .Where(x => x) //ログイン時
                .Subscribe(x =>
                {
                    //APIクライアント作成
                    _nicoliveApiClient = new NicoliveApiClient(_loginManager.CurrentUser);
                });

            //ログインできたら表示を切り替える
            _loginManager.IsLoggedIn.Subscribe(x =>
            {
                _loginPanel.SetActive(!x);
                _mainPanel.SetActive(x);
            });

            //対象番組設定
            _currentProgramId.Subscribe(x => { _nicoliveApiClient?.SetNicoliveProgramId(x); });

            IsSetProgramId
                .Subscribe(x => _blockPanel.SetActive(!x));
        }

        /// <summary>
        /// 番組IDを設定する
        /// </summary>
        public void SetTargetProgramId(string programId)
        {
            _currentRooms.Clear();
            if (_lvRegex.IsMatch(programId))
            {
                _currentProgramId.Value = programId;
            }
            else
            {
                Debug.LogError("不正な番組IDです");
            }
        }
    }
}