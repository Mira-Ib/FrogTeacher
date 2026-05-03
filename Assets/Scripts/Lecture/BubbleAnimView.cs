using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

// このスクリプトがアタッチされたオブジェクトに、自動的にCanvasGroupを追加します
[RequireComponent(typeof(CanvasGroup))]
public class BubbleAnimView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private AnimationCurve overshootCurve;

    // フェード制御用のCanvasGroup
    private CanvasGroup _canvasGroup;
    private Sequence _currentSequence;

    private void Awake()
    {
        // 自身のオブジェクトについているCanvasGroupを取得
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public async UniTask ShowBubbleAsync(CancellationToken token)
    {
        // 出現時は透明度を1（100%）にしておく
        _canvasGroup.alpha = 1f;

        bubbleRect.localScale = Vector3.zero;
        bubbleRect.gameObject.SetActive(true);

        _currentSequence = DOTween.Sequence()
            .Append(bubbleRect.DOScale(Vector3.one, 0.5f).SetEase(overshootCurve));

        await _currentSequence.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
    }

    /// <summary>
    /// 吹き出しをふわっと薄れて非表示にする処理
    /// </summary>
    public async UniTask HideBubbleAsync(CancellationToken token)
    {
        _currentSequence = DOTween.Sequence()
            // CanvasGroupの透明度を 0.5秒 かけて 0（透明）にする
            .Append(_canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                // 見えなくなったらオブジェクト自体をオフにする
                bubbleRect.gameObject.SetActive(false);
                // 次回表示された時のために透明度を戻しておく
                _canvasGroup.alpha = 1f;
            });

        await _currentSequence.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
    }

    public void FastForward()
    {
        _currentSequence?.Kill();

        // スキップされた場合は完全に非表示の状態へリセット
        bubbleRect.gameObject.SetActive(false);
        _canvasGroup.alpha = 1f;
    }
}