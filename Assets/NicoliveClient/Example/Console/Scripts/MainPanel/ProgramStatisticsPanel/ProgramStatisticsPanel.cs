using System;
using System.Linq;
using System.Text;
using NicoliveClient.Example;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    public class ProgramStatisticsPanel : MonoBehaviour
    {

        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _getButton;
        [SerializeField] private Text _programStatisticsLabel;

        void Start()
        {
            _getButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    _manager.NicoliveApiClient.GetProgramStatisticsAsync()
                        .Subscribe(result =>
                        {
                            _programStatisticsLabel.text =
                                string.Format("来場者数:{0} / コメント数:{1}", result.WatchCount, result.CommentCount);
                        }, Debug.LogError);
                });
        }
    }
}
