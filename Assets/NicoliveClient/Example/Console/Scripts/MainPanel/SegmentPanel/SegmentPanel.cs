using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    /// <summary>
    /// 番組時間に関係する操作
    /// </summary>
    public class SegmentPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _endButton;
        [SerializeField] private Button _getExtensionButton;
        [SerializeField] private Button _extendButton;
        [SerializeField] private Text _extendLabel;

        void Start()
        {
            _startButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    //番組開始
                    _manager.NicoliveApiClient
                        .StartProgramAsync()
                        .Subscribe(__ => { }, Debug.LogError);
                });

            _endButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    //番組終了
                    _manager.NicoliveApiClient
                        .EndProgramAsync()
                        .Subscribe(__ => { }, Debug.LogError);
                });

            _getExtensionButton
                .OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    //延長上限取得
                    _manager.NicoliveApiClient
                        .GetExtensionAsync()
                        .Subscribe(extensions =>
                        {
                            var max = extensions.Max(x => x.Minutes);
                            _extendLabel.text = max + "分延長可能";
                        }, Debug.LogError);
                });

            _extendButton
                .OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    //番組延長
                    _manager.NicoliveApiClient
                        .ExtendProgramAsync(30)
                        .Subscribe(__ => { }, Debug.LogError);
                });
        }
    }
}
