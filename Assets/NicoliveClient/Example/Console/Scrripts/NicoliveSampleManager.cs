using UniRx;
using UnityEngine;

namespace NicoliveClient.Example
{
    public class NicoliveSampleManager : MonoBehaviour
    {
        [SerializeField] private LoginManager _loginManager;
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _mainPanel;

        private NicoliveApiClient _nicoliveApiClient;
        public NicoliveApiClient NicoliveApiClient { get { return _nicoliveApiClient; } }

        private StringReactiveProperty _currentProgramId = new StringReactiveProperty("");

        /// <summary>
        /// 現在の対象番組ID
        /// </summary>
        public IReadOnlyReactiveProperty<string> CurrentProgramId
        {
            get { return _currentProgramId; }
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
            _currentProgramId.Subscribe(x =>
            {
                if (_nicoliveApiClient != null) _nicoliveApiClient.SetNicoliveProgramId(x);
            });


        }

        /// <summary>
        /// 現在のユーザに紐づく番組IDを取得する
        /// </summary>
        public IObservable<string> GetCurrentProgramIdAsync()
        {
            if (!_loginManager.IsLoggedIn.Value)
            {
                Debug.LogWarning("ログインしていません");
                return Observable.Return("");
            }

            //番組ID取得
            return _nicoliveApiClient.GetCurrentNicoliveProgramIdAsync();
        }

        /// <summary>
        /// 番組IDを設定する
        /// </summary>
        public void SetTargetProgramId(string programId)
        {
            _currentProgramId.Value = programId;
        }

    }
}
