using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening; // DOTweenを使用するために必要です

[RequireComponent(typeof(RectMask2D))]
public class HorizontalWipeMask : MonoBehaviour
{
    private RectMask2D targetMask;

    [Header("アニメーション設定")]
    [Tooltip("消去時のイージング（変化のスピード感）")]
    [SerializeField] private Ease wipeEase = Ease.Linear; // ★追加：インスペクタから選べるように

    private void Awake()
    {
        targetMask = GetComponent<RectMask2D>();
    }

    public async UniTask WipeOutAsync(float duration, CancellationToken token)
    {
        float currentWidth = targetMask.rectTransform.rect.width;

        await DOTween.To(
            () => targetMask.padding,
            x => targetMask.padding = x,
            new Vector4(currentWidth, 0, 0, 0),
            duration
        )
        .SetEase(wipeEase) // ★修正：インスペクタで設定したイージングを適用
        .WithCancellation(token);
    }

    public void ResetMask()
    {
        if (targetMask != null)
        {
            targetMask.padding = Vector4.zero;
        }
    }
}