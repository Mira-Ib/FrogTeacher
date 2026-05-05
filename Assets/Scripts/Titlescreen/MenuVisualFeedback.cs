using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class MenuVisualFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [Header("演出の設定")]
    [Tooltip("選択時にどれくらい大きくするか（例：1.1倍）")]
    [SerializeField] private float scaleUpSize = 1.1f;
    [Tooltip("アニメーションにかかる時間")]
    [SerializeField] private float animationDuration = 0.2f;

    [Header("矢印の参照")]
    [Tooltip("この項目が選ばれた時に追従してくる矢印のRectTransform")]
    [SerializeField] private RectTransform arrowObject;

    private Vector3 originalScale;
    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    // マウスが乗った時の処理
    public void OnPointerEnter(PointerEventData eventData)
    {
        // マウスホバーで、Unity標準の「選択状態」にする（これでOnSelectが呼ばれます）
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    // 選択状態になった時（キーボード操作時もここが呼ばれる）
    public void OnSelect(BaseEventData eventData)
    {
        // 1. 少し大きくなる
        transform.DOScale(originalScale * scaleUpSize, animationDuration).SetEase(Ease.OutQuad);

        // 2. 矢印を自分の横へ移動させる
        if (arrowObject != null)
        {
            // ★追加：矢印が非アクティブなら、ここで表示をオンにする
            arrowObject.gameObject.SetActive(true);

            arrowObject.DOAnchorPosY(myRectTransform.anchoredPosition.y, animationDuration)
                       .SetEase(Ease.OutCubic);
        }
    }

    // 選択が外れた時
    public void OnDeselect(BaseEventData eventData)
    {
        // 元のサイズに戻る
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad);
    }
}