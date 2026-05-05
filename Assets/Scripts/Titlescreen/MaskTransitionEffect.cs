using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MaskTransitionEffect : MonoBehaviour
{
    [Tooltip("背景や未選択のメニューなど、消去される要素が入っている親")]
    [SerializeField] private RectTransform maskedContainer;

    [Tooltip("マスクの影響を受けない、上の階層にある親")]
    [SerializeField] private RectTransform safeContainer;

    [Tooltip("画面全体を左から右に消すマスク（maskedContainerにアタッチされている想定）")]
    [SerializeField] private HorizontalWipeMask backgroundWipeMask;

    // ★追加：どのUIを避難させたか記憶しておく変数
    private GameObject escapedMenu;
    private GameObject escapedFrog;

    public async UniTask EraseBackgroundExceptAsync(GameObject selectedMenu, GameObject frogTeacher, CancellationToken token)
    {
        // 避難させるオブジェクトを変数に記憶しておく
        escapedMenu = selectedMenu;
        escapedFrog = frogTeacher;

        // マスクの外（safeContainer）に避難させる
        if (escapedMenu != null)
            escapedMenu.transform.SetParent(safeContainer, true);

        if (escapedFrog != null)
            escapedFrog.transform.SetParent(safeContainer, true);

        // 背景を消去
        await backgroundWipeMask.WipeOutAsync(1.0f, token);
    }

    // ★追加：タイトルに戻ってきた時のリセット処理
    public void ResetMaskAndParents()
    {
        // 1. 避難させていたUIを、元の階層（maskedContainer）に戻す
        if (escapedMenu != null)
        {
            escapedMenu.transform.SetParent(maskedContainer, true);
            escapedMenu = null; // 戻したら空にする
        }

        if (escapedFrog != null)
        {
            escapedFrog.transform.SetParent(maskedContainer, true);
            escapedFrog = null;
        }

        // 2. 画面全体を覆っていたマスクを全開（初期状態）に戻す
        if (backgroundWipeMask != null)
        {
            backgroundWipeMask.ResetMask();
        }
    }
}