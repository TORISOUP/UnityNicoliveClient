using UnityEngine;
using UniRx;
using UnityEngine.UI;

namespace NicoliveClient.Example
{
    /// <summary>
    /// コメントを描画する（重い）
    /// </summary>
    public class CommentRenderPanel : MonoBehaviour
    {
        [SerializeField] private CommentPanel _commentPanelManager;

        [SerializeField] private Transform _renderAreaParent;

        void Start()
        {
            var renderPanelCount = _renderAreaParent.childCount;

            _commentPanelManager.OnCommentRecieved
                .Subscribe(message =>
                {
                    //最後尾のPanelを持ってくる
                    var last = _renderAreaParent.GetChild(renderPanelCount);

                    //Textを取得
                    var text = last.GetComponentInChildren<Text>();

                    text.text = message;

                    //描画したパネルを先頭に持ってくる
                    last.SetSiblingIndex(0);
                });
        }

    }
}
