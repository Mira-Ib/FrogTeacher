using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro; // ★追加：TextMeshProを操作するために必要です

[RequireComponent(typeof(RectTransform))]
public class MenuVisualFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [Header("拡大演出の設定")]
    [SerializeField] private bool useScale = true;
    [SerializeField] private float scaleUpSize = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("矢印の参照")]
    [SerializeField] private RectTransform arrowObject;

    [Header("正方形枠（フレーム）の参照")]
    [SerializeField] private RectTransform squareFrameObject;

    [Header("選択解除時の設定")]
    [SerializeField] private bool hideVisualsOnDeselect = false;

    // ★新たに追加した設定
    [Header("テキストの装飾設定")]
    [Tooltip("選択時に下線を付けたいテキスト（不要な場合は空欄）")]
    [SerializeField] private TextMeshProUGUI targetTextToUnderline;

    private Vector3 originalScale;
    private RectTransform myRectTransform;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // 1. 少し大きくなる
        if (useScale)
        {
            transform.DOScale(originalScale * scaleUpSize, animationDuration)
                     .SetEase(Ease.OutQuad)
                     .SetUpdate(true);
        }

        // 2. 矢印を移動
        if (arrowObject != null)
        {
            arrowObject.gameObject.SetActive(true);
            arrowObject.DOAnchorPosY(myRectTransform.anchoredPosition.y, animationDuration)
                       .SetEase(Ease.OutCubic)
                       .SetUpdate(true);
        }

        // 3. 正方形枠を移動
        if (squareFrameObject != null)
        {
            squareFrameObject.gameObject.SetActive(true);
            squareFrameObject.DOAnchorPos(myRectTransform.anchoredPosition, animationDuration)
                             .SetEase(Ease.OutCubic)
                             .SetUpdate(true);
        }

        // ★追加：4. テキストに下線を付ける
        if (targetTextToUnderline != null)
        {
            // | (OR演算子) を使うことで、元の太字などの設定を壊さずに下線だけを追加します
            targetTextToUnderline.fontStyle |= FontStyles.Underline;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // 元のサイズに戻る
        if (useScale)
        {
            transform.DOScale(originalScale, animationDuration)
                     .SetEase(Ease.OutQuad)
                     .SetUpdate(true);
        }

        // 選択が外れたらカーソルや枠を隠す
        if (hideVisualsOnDeselect)
        {
            if (arrowObject != null) arrowObject.gameObject.SetActive(false);
            if (squareFrameObject != null) squareFrameObject.gameObject.SetActive(false);
        }

        // ★追加：テキストの下線を外す
        if (targetTextToUnderline != null)
        {
            // & ~ (AND NOT演算子) を使うことで、下線だけを綺麗に取り除きます
            targetTextToUnderline.fontStyle &= ~FontStyles.Underline;
        }
    }
}