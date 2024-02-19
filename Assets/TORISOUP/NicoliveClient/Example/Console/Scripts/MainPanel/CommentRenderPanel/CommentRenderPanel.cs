using UnityEngine;
using UnityEngine.UI;
using R3;

namespace TORISOUP.NicoliveClient.Example.Console.Scripts.MainPanel.CommentRenderPanel
{
    /// <summary>
    /// コメントを描画する（重い）
    /// </summary>
    public class CommentRenderPanel : MonoBehaviour
    {
        [SerializeField] private CommentPanel.CommentPanel _commentPanelManager;

        [SerializeField] private Transform _renderAreaParent;

        void Start()
        {
            var renderPanelCount = _renderAreaParent.childCount;

            _commentPanelManager.OnCommentReceived
                .Subscribe(message =>
                {

                    //最後尾のPanelを持ってくる
                    var last = _renderAreaParent.GetChild(renderPanelCount - 1);

                    //Textを取得
                    var text = last.GetComponentInChildren<Text>();

                    text.text = message.Content;

                    //描画したパネルを先頭に持ってくる
                    last.SetSiblingIndex(0);
                });
        }

    }
}
