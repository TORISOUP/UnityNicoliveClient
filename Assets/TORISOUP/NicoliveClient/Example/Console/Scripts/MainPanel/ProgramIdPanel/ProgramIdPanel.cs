using System;
using UniRx;
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
            _getCurrentProgramIdButton
                .OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(3)) //連打防止
                .Subscribe(_ =>
                {
                    //取得したIDは一旦UIに反映
                    _manager.GetCurrentProgramIdAsync()
                        .Subscribe(x => _programIdInputField.text = x, Debug.LogError);
                });

            //設定ボタンが押されたらUIの値を反映する
            _setProgramIdButton.OnClickAsObservable()
                .Subscribe(_ => _manager.SetTargetProgramId(_programIdInputField.text));

            //現在の操作対象lv表示
            _manager.CurrentProgramId
                .SubscribeToText(_currentProgramIdText);
        }

    }
}
