using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.OperatorCommentPanel
{
    /// <summary>
    /// 運営コメントパネルのUI
    /// </summary>
    public class OperatorCommentPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private InputField _nameInputField;
        [SerializeField] private InputField _bodyInputField;
        [SerializeField] private Dropdown _colorDropdown;
        [SerializeField] private Toggle _isPermanentToggle;
        [SerializeField] private Button _sendButton;
        [SerializeField] private Button _deleteButton;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            _sendButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        //運営コメント送信
                        await _manager.NicoliveApiClient
                            .SendOperatorCommentAsync(
                                _bodyInputField.text,
                                _nameInputField.text,
                                _colorDropdown.options[_colorDropdown.value].text,
                                _isPermanentToggle.isOn, c);

                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: c);
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Debug.LogError(e);
                    }
                }, cancellationToken: ct)
                .Forget();

            _deleteButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        //運営コメント削除
                        await _manager.NicoliveApiClient
                            .DeleteOperatorCommentAsync(c);
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