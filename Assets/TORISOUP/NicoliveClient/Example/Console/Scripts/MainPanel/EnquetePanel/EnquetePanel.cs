using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.EnquetePanel
{
    public class EnquetePanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button StartButton;
        [SerializeField] private Button ShowResultButton;
        [SerializeField] private Button EndButton;

        [SerializeField] private InputField QuestionTitleInputField;
        [SerializeField] private InputField[] itemInputFields;

        [SerializeField] private Text ResultLabel;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            //アンケート開始
            StartButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        var title = QuestionTitleInputField.text;
                        var items = itemInputFields.Select(x => x.text);

                        await _manager.NicoliveApiClient
                            .StartEnqueteAsync(title, items, c);

                        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: c);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, cancellationToken: ct)
                .Forget();

            //アンケート結果の表示＆取得
            ShowResultButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        var result = await _manager.NicoliveApiClient.ShowResultEnqueteAsync(c);

                        var message = result.Items.Select(x => $"{x.Name}:{x.Rate}%")
                            .Aggregate((p, c) => p + " / " + c);
                        ResultLabel.text = message;
                        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: c);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, cancellationToken: ct)
                .Forget();


            //アンケートの終了
            EndButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        await _manager.NicoliveApiClient.FinishEnqueteAsync(c);
                        ResultLabel.text = "アンケートを終了しました";
                        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: c);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, cancellationToken: ct)
                .Forget();
        }
    }
}