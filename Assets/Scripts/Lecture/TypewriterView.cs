using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TypewriterView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float charactersPerSecond = 20f;

    private Tween _typewriterTween;

    public async UniTask PlayTypewriterAsync(string content, CancellationToken token)
    {
        textMesh.text = content;
        textMesh.maxVisibleCharacters = 0;

        // 全文字表示にかかる時間を計算
        float duration = content.Length / charactersPerSecond;

        // DOTweenでmaxVisibleCharactersを0から文字数まで変化させる
        _typewriterTween = DOTween.To(
            () => textMesh.maxVisibleCharacters,
            x => textMesh.maxVisibleCharacters = x,
            content.Length,
            duration
        ).SetEase(Ease.Linear);

        // 再生終了またはキャンセルを待機
        await _typewriterTween.WithCancellation(token);
    }

    public void FastForward()
    {
        _typewriterTween?.Kill();
        textMesh.maxVisibleCharacters = textMesh.text.Length;
    }
}