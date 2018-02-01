using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
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
            _sendButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1)) //連打防止
                .Subscribe(_ =>
                {

                    //運営コメント送信
                    _manager.NicoliveApiClient
                        .SendOperatorCommentAsync(
                            _bodyInputField.text,
                            _nameInputField.text,
                            _colorDropdown.options[_colorDropdown.value].text,
                            _isPermanentToggle.isOn
                        ).Subscribe(__ => { }, Debug.LogError);
                });

            _deleteButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1)) //連打防止
                .Subscribe(_ =>
                {

                    //運営コメント削除
                    _manager.NicoliveApiClient
                        .DeleteOperatorCommentAsync()
                        .Subscribe(__ => { }, Debug.LogError);
                });
        }

    }
}
