using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.ProgramStatisticsPanel
{
    public class ProgramStatisticsPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _getButton;
        [SerializeField] private Text _programStatisticsLabel;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            var command = _manager.IsSetProgramId.ToReactiveCommand();

            _getButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(5))
                .Subscribe(_ => command.Execute())
                .AddTo(ct);

            command.Subscribe(_ =>
                {
                    UniTask.Void(async () =>
                    {
                        var result = await _manager.NicoliveApiClient.GetProgramStatisticsAsync(ct);
                        _programStatisticsLabel.text =
                            $"来場者数:{result.WatchCount} / コメント数:{result.CommentCount}";
                    });
                })
                .AddTo(ct);
        }
    }
}