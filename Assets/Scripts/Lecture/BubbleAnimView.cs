using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(CanvasGroup))]
public class BubbleAnimView : MonoBehaviour, ILecturePlayable
{
    [Header("UI参照")]
    [SerializeField] private RectTransform bubbleRect;

    [Header("出現アニメーション設定")]
    // AnimationCurveを削除し、DOTweenのEaseをインスペクタで選べるようにしました
    // デフォルトで OutBack（にょきっと膨らむ）がセットされます
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private float showDuration = 0.5f; // 出現にかかる秒数

    [Header("退出アニメーション設定")]
    [SerializeField] private float hideDuration = 0.5f; // フェードアウトにかかる秒数

    private CanvasGroup _canvasGroup;
    private Sequence _currentSequence;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public async UniTask ShowBubbleAsync(CancellationToken token)
    {
        _canvasGroup.alpha = 1f;
        bubbleRect.localScale = Vector3.zero;
        bubbleRect.gameObject.SetActive(true);

        _currentSequence = DOTween.Sequence()
            // SetEaseにインスペクタで設定したEaseを適用
            .Append(bubbleRect.DOScale(Vector3.one, showDuration).SetEase(showEase));

        await _currentSequence.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);
    }

    public async UniTask HideBubbleAsync(CancellationToken token)
    {
        _currentSequence = DOTween.Sequence()
            .Append(_canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                bubbleRect.gameObject.SetActive(false);
                _canvasGroup.alpha = 1f;
            });

        await _currentSequence.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);
    }

    public void FastForward()
    {
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
        }

        bubbleRect.gameObject.SetActive(false);
        _canvasGroup.alpha = 1f;
    }
}