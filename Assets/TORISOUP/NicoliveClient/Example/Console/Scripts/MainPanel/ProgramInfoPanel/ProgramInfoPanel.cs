using System;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.ProgramInfoPanel
{
    public class ProgramInfoPanel : MonoBehaviour
    {
        [SerializeField] private NicoliveSampleManager _manager;

        [SerializeField] private Button _getButton;
        [SerializeField] private Text _programInfoLabel;

        void Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            _getButton.OnClickAsAsyncEnumerable(ct)
                .ForEachAwaitWithCancellationAsync(async (_, c) =>
                {
                    try
                    {
                        var programInfo = await _manager.NicoliveApiClient.GetProgramInfoAsync(c);
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
                            _manager.CurrentRooms.TryAdd(room.ViewUri, room);
                        }
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