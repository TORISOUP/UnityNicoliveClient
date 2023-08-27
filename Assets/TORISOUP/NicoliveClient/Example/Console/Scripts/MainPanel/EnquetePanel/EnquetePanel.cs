using System;
using System.Linq;
using UniRx;
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
            //アンケート開始
            StartButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(3)) //連打防止
                .Subscribe(_ =>
                {
                    var title = QuestionTitleInputField.text;
                    var items = itemInputFields.Select(x => x.text);

                    _manager.NicoliveApiClient
                        .StartEnqueteAsync(title, items)
                        .Subscribe(__ => ResultLabel.text = "アンケートを開始しました", Debug.LogError);

                });

            //アンケート結果の表示＆取得
            ShowResultButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(3)) //連打防止
                .Subscribe(_ =>
                {
                    _manager.NicoliveApiClient
                        .ShowResultEnqueteAsync()
                        .Subscribe(result =>
                        {
                            var message = result.Items.Select(x => string.Format("{0}:{1}%", x.Name, x.Rate))
                                .Aggregate((p, c) => p + " / " + c);
                            ResultLabel.text = message;
                        }, Debug.LogError);
                });


            //アンケートの終了
            EndButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(3)) //連打防止
                .Subscribe(_ =>
                {
                    _manager.NicoliveApiClient
                        .FinishEnqueteAsync()
                        .Subscribe(__ => ResultLabel.text = "アンケートを終了しました", Debug.LogError);
                });

        }

    }
}
