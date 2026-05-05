using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class NextScreenPanel : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("演出の設定")]
    [SerializeField] private float startPosY = 1500f; // 画面外（上）
    [SerializeField] private float targetPosY = 0f;   // 定位置
    [SerializeField] private float animationDuration = 0.8f;

    [Header("マスクの参照")]
    [Tooltip("このパネル自身を消去するためのマスク")]
    [SerializeField] private HorizontalWipeMask wipeMask;

    private void Awake()
    {
        EnsureInitialized(); // Awakeでも一応呼んでおく
    }

    // ★追加：確実に取得するためのメソッド
    private void EnsureInitialized()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    // 上から降ってくる（変更なし）
    public async UniTask DropInAsync(CancellationToken token)
    {
        EnsureInitialized(); // ★追加
        gameObject.SetActive(true);
        wipeMask.ResetMask(); // 降りてくる時はマスクを全開にしておく
        await rectTransform.DOAnchorPosY(targetPosY, animationDuration)
            .SetEase(Ease.OutBack)
            .WithCancellation(token);
    }

    // ★変更：上に上がるのではなく、マスクで左から右へ消去する
    public async UniTask WipeOutAsync(CancellationToken token)
    {
        EnsureInitialized(); // ★追加
        await wipeMask.WipeOutAsync(animationDuration, token);
        gameObject.SetActive(false);
    }

    public void ResetToStartPos()
    {
        EnsureInitialized(); // ★追加
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startPosY);
        gameObject.SetActive(false);
    }
}