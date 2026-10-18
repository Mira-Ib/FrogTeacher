using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TMP_Ruby;
using UnityEngine;

/// <summary>
/// 授業開始直後に、カエル先生の心の声などを一時表示するビューです。
/// </summary>
public class OpeningMonologueView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI monologueText;
    [SerializeField] private TextMeshProRuby monologueRuby;
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    private Sequence _sequence;

    private void Awake()
    {
        HideImmediately();
    }

    public async UniTask PlayAsync(string message, float displayDuration, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _sequence?.Kill();

        monologueText.gameObject.SetActive(true);
        if (monologueRuby != null)
        {
            monologueRuby.Text = message;
        }
        else
        {
            // シーン側の設定が欠けた場合でも、通常テキストとしては表示する。
            monologueText.text = message;
        }
        canvasGroup.alpha = 0f;

        // 表示時間が短く設定されても、フェードインとフェードアウトが収まるようにする。
        float actualFadeDuration = Mathf.Min(fadeDuration, Mathf.Max(0f, displayDuration * 0.5f));
        float holdDuration = Mathf.Max(0f, displayDuration - actualFadeDuration * 2f);

        _sequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, actualFadeDuration).SetEase(Ease.OutQuad))
            .AppendInterval(holdDuration)
            .Append(canvasGroup.DOFade(0f, actualFadeDuration).SetEase(Ease.InQuad))
            .OnComplete(HideImmediately);

        await _sequence.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);
    }

    public void FastForward()
    {
        _sequence?.Kill();
        HideImmediately();
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }

    private void HideImmediately()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (monologueText != null)
        {
            monologueText.gameObject.SetActive(false);
        }
    }
}
