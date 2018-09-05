using System;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    public class ScheduledProgramPanel : MonoBehaviour
    {
        [SerializeField]
        private NicoliveSampleManager _manager;

        [SerializeField]
        private Button _getButton;

        [SerializeField]
        private Text _resultLabel;

        void Start()
        {
            _getButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    _manager.NicoliveApiClient.GetScheduledProgramListAsync()
                        .Subscribe(result =>
                        {
                            if (result.Length == 0)
                            {
                                _resultLabel.text = "現在予定されている番組はありません";
                                return;
                            }

                            var builder = new StringBuilder();
                            foreach (var schedule in result)
                            {
                                builder.Append(string.Format("{0}:{1}:{2}\n", schedule.ProgramId, schedule.SocialGroupId, schedule.Status));
                            }

                            _resultLabel.text = builder.ToString();
                        }, Debug.LogError);
                });
        }
    }
}
