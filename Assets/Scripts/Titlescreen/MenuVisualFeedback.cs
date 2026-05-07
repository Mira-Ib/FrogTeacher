using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

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
        // 1. 拡大演出
        if (useScale)
        {
            transform.DOScale(originalScale * scaleUpSize, animationDuration)
                     .SetEase(Ease.OutQuad)
                     .SetUpdate(true);
        }

        // 2. 矢印の移動（★変更：DOTweenを使った滑らかなアニメーションに戻す）
        if (arrowObject != null)
        {
            arrowObject.gameObject.SetActive(true);
            arrowObject.SetAsLastSibling(); // 最前面へ

            // DOTweenでスムーズに追従させる
            arrowObject.DOAnchorPosY(myRectTransform.anchoredPosition.y, animationDuration)
                       .SetEase(Ease.OutCubic)
                       .SetUpdate(true);
        }

        // 3. 正方形枠の移動（要望通り、ここは直接座標を代入して瞬時に移動）
        if (squareFrameObject != null)
        {
            squareFrameObject.gameObject.SetActive(true);
            squareFrameObject.SetAsLastSibling();

            // アニメーションなしで即座に吸い付く
            squareFrameObject.anchoredPosition = myRectTransform.anchoredPosition;
        }

        // 4. テキストに下線を付ける
        if (targetTextToUnderline != null)
        {
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

        // 選択が外れた時の表示制御
        if (hideVisualsOnDeselect)
        {
            if (arrowObject != null) arrowObject.gameObject.SetActive(false);
            if (squareFrameObject != null) squareFrameObject.gameObject.SetActive(false);
        }

        // テキストの下線を外す
        if (targetTextToUnderline != null)
        {
            targetTextToUnderline.fontStyle &= ~FontStyles.Underline;
        }
    }
}