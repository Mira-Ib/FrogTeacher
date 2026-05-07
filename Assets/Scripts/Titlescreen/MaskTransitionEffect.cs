using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MaskTransitionEffect : MonoBehaviour
{
    [Tooltip("背景や未選択のメニューなど、消去される要素が入っている親")]
    [SerializeField] private RectTransform maskedContainer;

    [Tooltip("マスクの影響を受けない、上の階層にある親")]
    [SerializeField] private RectTransform safeContainer;

    [Tooltip("画面全体を左から右に消すマスク")]
    [SerializeField] private HorizontalWipeMask backgroundWipeMask;

    // ★変更：Listではなく、Dictionaryを使って「GameObject」と「元の親Transform」をセットで記憶する
    private Dictionary<GameObject, Transform> _originalParents = new Dictionary<GameObject, Transform>();

    /// <summary>
    /// 指定したオブジェクトを SafeContainer に避難させる
    /// </summary>
    public void ProtectObjects(params GameObject[] targetsToProtect)
    {
        foreach (var obj in targetsToProtect)
        {
            // まだ辞書に登録されていなければ（重複登録防止）
            if (obj != null && !_originalParents.ContainsKey(obj))
            {
                // ★追加：避難させる「前」に、現在の親（Buttonsなど）を記憶しておく
                _originalParents[obj] = obj.transform.parent;

                // 避難先に移動させる
                obj.transform.SetParent(safeContainer, true);
            }
        }
    }

    /// <summary>
    /// マスクアニメーションを実行する
    /// </summary>
    public async UniTask WipeOutAsync(float duration, CancellationToken token)
    {
        if (backgroundWipeMask != null)
        {
            await backgroundWipeMask.WipeOutAsync(duration, token);
        }
    }

    /// <summary>
    /// 避難していたUIを元の階層に戻し、マスクをリセットする
    /// </summary>
    public void ResetMaskAndParents()
    {
        // 1. 記憶しているすべてのUIを、それぞれ固有の「元の親」に戻す
        foreach (var kvp in _originalParents)
        {
            GameObject obj = kvp.Key;
            Transform originalParent = kvp.Value;

            if (obj != null && originalParent != null)
            {
                // まとめてmaskedContainerではなく、記憶していた元の親（Buttons等）に戻す
                obj.transform.SetParent(originalParent, true);
            }
        }

        // 戻し終わったら辞書を空にする
        _originalParents.Clear();

        // 2. マスクを全開（初期状態）に戻す
        if (backgroundWipeMask != null)
        {
            backgroundWipeMask.ResetMask();
        }
    }
}