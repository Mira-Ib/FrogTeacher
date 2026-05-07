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
        // マウスホバーで、Unity標準の「選択状態」にする
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    // 選択状態になった時（キーボード操作時もここが呼ばれる）
    public void OnSelect(BaseEventData eventData)
    {
        // 1. ボタンが少し大きくなる演出
        transform.DOScale(originalScale * scaleUpSize, animationDuration).SetEase(Ease.OutQuad);

        // 2. 矢印を自分の横へ移動させる
        if (arrowObject != null)
        {
            // --- ★修正ポイントここから ---

            // ① 矢印を表示状態にする
            arrowObject.gameObject.SetActive(true);

            // ② 矢印をヒエラルキーの「一番下」に移動させ、強制的に最前面に描画する
            // （uGUIでは同じ親の中では下にあるものほど手前に表示されるため）
            arrowObject.SetAsLastSibling();

            // ③ 矢印の移動アニメーション
            arrowObject.DOAnchorPosY(myRectTransform.anchoredPosition.y, animationDuration)
                       .SetEase(Ease.OutCubic);

            // --- ★修正ポイントここまで ---
        }
    }

    // 選択が外れた時
    public void OnDeselect(BaseEventData eventData)
    {
        // 元のサイズに戻る
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuad);
    }
}