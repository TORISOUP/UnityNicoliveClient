using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    public class BspCommentPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private InputField _nameInputField;
        [SerializeField] private InputField _bodyInputField;
        [SerializeField] private Dropdown _colorDropdown;
        [SerializeField] private Button _sendButton;
        void Start()
        {
            _sendButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1)) //連打防止
                .Subscribe(_ =>
                {

                    //BSPコメント送信
                    _manager.NicoliveApiClient
                        .SendBspCommentAsync(
                            _bodyInputField.text,
                            _nameInputField.text,
                            _colorDropdown.options[_colorDropdown.value].text
                        ).Subscribe(__ => { }, Debug.LogError);
                });

        }
    }
}
