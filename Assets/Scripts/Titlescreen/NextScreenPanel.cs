using System.Threading;
using UnityEngine;
// ★追加：EventSystemを使うために必要です
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class NextScreenPanel : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("演出の設定")]
    [SerializeField] private float startPosY = 1500f;
    [SerializeField] private float targetPosY = 0f;
    [SerializeField] private float animationDuration = 0.8f;

    [Header("マスクの参照")]
    [SerializeField] private HorizontalWipeMask wipeMask;

    // ★追加：この画面が開いた時にフォーカスを当てるUI
    [Header("UIナビゲーション")]
    [Tooltip("この画面が開ききった時に最初に選択状態にするUI（スライダーや戻るボタン等）")]
    [SerializeField] private GameObject firstSelectedElement;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public async UniTask DropInAsync(CancellationToken token)
    {
        EnsureInitialized();
        gameObject.SetActive(true);
        wipeMask.ResetMask();

        await rectTransform.DOAnchorPosY(targetPosY, animationDuration)
            .SetEase(Ease.OutBack)
            .WithCancellation(token);

        // ★追加：アニメーションが終わって画面が定位置についたらフォーカスを当てる
        if (firstSelectedElement != null)
        {
            // 一旦nullを入れてリセットしてから設定すると、より確実にフォーカスが切り替わります
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedElement);
        }
    }

    public async UniTask WipeOutAsync(CancellationToken token)
    {
        EnsureInitialized();
        await wipeMask.WipeOutAsync(animationDuration, token);
        gameObject.SetActive(false);
    }

    public void ResetToStartPos()
    {
        EnsureInitialized();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startPosY);
        gameObject.SetActive(false);
    }
}