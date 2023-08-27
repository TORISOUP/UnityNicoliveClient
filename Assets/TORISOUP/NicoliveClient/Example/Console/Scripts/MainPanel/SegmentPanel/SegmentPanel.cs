using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.SegmentPanel
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
            var ct = this.GetCancellationTokenOnDestroy();

            _startButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(
                    async (_, c) =>
                    {
                        try
                        {
                            await _manager.NicoliveApiClient.StartProgramAsync(c);
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            Debug.LogError(e);
                        }
                    }, ct)
                .Forget();

            _endButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(
                    async (_, c) =>
                    {
                        try
                        {
                            await _manager.NicoliveApiClient.EndProgramAsync(c);
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            Debug.LogError(e);
                        }
                    }, ct)
                .Forget();

            _getExtensionButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(
                    async (_, c) =>
                    {
                        try
                        {
                            var extensions =
                                await _manager.NicoliveApiClient.GetExtensionAsync(c);
                            var max = extensions.Max(x => x.Minutes);
                            _extendLabel.text = max + "分延長可能";
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            Debug.LogError(e);
                        }
                    }, ct)
                .Forget();

            _extendButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(
                    async (_, c) =>
                    {
                        try
                        {
                            await _manager.NicoliveApiClient.ExtendProgramAsync(30, c);
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