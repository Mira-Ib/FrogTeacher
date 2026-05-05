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

    // 引数で「ユーザーが選んだ選択肢」と「カエル先生」のGameObjectを受け取る
    public async UniTask EraseBackgroundExceptAsync(GameObject selectedMenu, GameObject frogTeacher, CancellationToken token)
    {
        // 1. 選ばれたメニューとカエル先生を、マスクの外（safeContainer）に避難させる
        // 第二引数の true は「見た目の位置（WorldPosition）を維持する」という意味です
        if (selectedMenu != null)
            selectedMenu.transform.SetParent(safeContainer, true);

        if (frogTeacher != null)
            frogTeacher.transform.SetParent(safeContainer, true);

        // 2. 背景（残された未選択のメニュー等を含む）を左から右へ消去
        await backgroundWipeMask.WipeOutAsync(1.0f, token);
    }
}