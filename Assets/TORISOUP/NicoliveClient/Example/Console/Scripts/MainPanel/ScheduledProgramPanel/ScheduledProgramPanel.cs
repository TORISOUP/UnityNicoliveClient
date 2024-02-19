using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.ScheduledProgramPanel
{
    public class ScheduledProgramPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _getButton;

        [SerializeField] private Text _resultLabel;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            _getButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        var list = await _manager.NicoliveApiClient.GetScheduledProgramListAsync(c);
                        if (list.Length == 0)
                        {
                            _resultLabel.text = "現在予定されている番組はありません";
                            return;
                        }

                        var builder = new StringBuilder();
                        foreach (var schedule in list)
                        {
                            builder.Append($"{schedule.ProgramId}:{schedule.SocialGroupId}:{schedule.Status}\n");
                        }

                        _resultLabel.text = builder.ToString();

                        // Delay
                        await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: c);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, ct)
                .Forget();
        }
    }
}