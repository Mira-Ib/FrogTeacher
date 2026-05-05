using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class FrogTeacherAnimator : MonoBehaviour
{
    private RectTransform rectTransform;

    // ★追加：初期位置を記憶する変数
    private Vector2 originalAnchoredPosition;

    [Header("アニメーション設定")]
    [SerializeField] private float jumpPower = 50f;
    [SerializeField] private float jumpDuration = 0.3f;
    [SerializeField] private float exitDistance = 1500f;
    [SerializeField] private float exitDuration = 0.8f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // ★追加：起動時に、インスペクタで配置されている初期座標を保存しておく
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public async UniTask JumpAndExitAsync(CancellationToken token)
    {
        await rectTransform.DOJumpAnchorPos(
            rectTransform.anchoredPosition,
            jumpPower,
            numJumps: 1,
            duration: jumpDuration
        ).WithCancellation(token);

        await rectTransform.DOAnchorPosX(
            rectTransform.anchoredPosition.x + exitDistance,
            exitDuration
        )
        .SetEase(Ease.Linear)
        .WithCancellation(token);
    }

    // ★追加：タイトルに戻った時に呼び出されるリセット処理
    public void ResetPosition()
    {
        // 記憶しておいた初期位置へ一瞬で戻す
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}