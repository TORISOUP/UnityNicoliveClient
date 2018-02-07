using System;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    public class ProgramInfoPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _getButton;
        [SerializeField] private Text _programInfoLabel;

        void Start()
        {
            _getButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(5))
                .Subscribe(_ =>
                {
                    _manager.NicoliveApiClient.GetProgramInfoAsync()
                        .Subscribe(programInfo =>
                        {
                            //番組情報を取得して一部を表示する
                            var builder = new StringBuilder();
                            builder.Append("title:" + programInfo.Title + "\n");
                            builder.Append("type:" + programInfo.SocialGroup.Type.ToString() + "\n");
                            builder.Append("socialGroupId:" + programInfo.SocialGroup.Id + "\n");
                            builder.Append("tags:" + programInfo.Categories.Aggregate((p, c) => p + "," + c) + "\n");
                            builder.Append("room count:" + programInfo.Rooms.Length + "\n");
                            builder.Append("status:" + programInfo.Status.ToString() + "\n");
                            _programInfoLabel.text = builder.ToString();

                            //部屋一覧を登録する
                            foreach (var room in programInfo.Rooms)
                            {
                                if (!_manager.CurrentRooms.ContainsKey(room.Id))
                                {
                                    _manager.CurrentRooms[room.Id] = room;
                                }
                            }
                        }, Debug.LogError);
                });
        }
    }
}
