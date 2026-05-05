using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class FormulaView : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private HorizontalWipeMask wipeMask;

    [Header("演出の調整")]
    [Tooltip("マスクで消去するのにかかる時間（秒）")]
    [SerializeField] private float eraseDuration = 0.5f; // ★ここを追加

    public async UniTask PlayFlipbookAsync(Sprite[] frames, float duration, CancellationToken token)
    {
        wipeMask.ResetMask();
        targetImage.enabled = true;

        foreach (var frame in frames)
        {
            targetImage.sprite = frame;
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token);
        }
    }

    public async UniTask EraseByMaskAsync(CancellationToken token)
    {
        // ★固定の0.5fではなく、eraseDuration を使うように変更
        await wipeMask.WipeOutAsync(eraseDuration, token);

        targetImage.enabled = false;
    }
}