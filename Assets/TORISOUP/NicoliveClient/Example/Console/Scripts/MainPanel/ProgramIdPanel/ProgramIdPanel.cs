using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.ProgramIdPanel
{
    /// <summary>
    /// 番組ID部分のUI表示
    /// </summary>
    public class ProgramIdPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private InputField _programIdInputField;
        [SerializeField] private Button _getCurrentProgramIdButton;
        [SerializeField] private Button _setProgramIdButton;
        [SerializeField] private Text _currentProgramIdText;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            _getCurrentProgramIdButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        //取得したIDは一旦UIに反映
                        var programs = await _manager.GetCurrentProgramIdAsync(c);
                        if (programs.Length == 0)
                        {
                            Debug.LogError("番組IDが取得できませんでした");
                            _programIdInputField.text = "";
                            return;
                        }
                        _programIdInputField.text = programs.Last();
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, cancellationToken: ct)
                .Forget();

            //設定ボタンが押されたらUIの値を反映する
            _setProgramIdButton.OnClickAsObservable()
                .Subscribe(_ => _manager.SetTargetProgramId(_programIdInputField.text))
                .AddTo(ct);

            //現在の操作対象lv表示
            _manager.CurrentProgramId
                .SubscribeToText(_currentProgramIdText)
                .AddTo(ct);
        }
    }
}