using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SymbolAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private HorizontalWipeMask wipeMask;
    [SerializeField] private Sprite[] frames;

    [Header("タイミングの調整")]
    [SerializeField] private float frameDuration = 0.1f;
    [SerializeField] private float startDelay = 0f;
    [Tooltip("パラパラ漫画が終わってから、消し始めるまでの待機時間（秒）")]
    [SerializeField] private float waitTimeBeforeErase = 1.0f; // ★追加
    [Tooltip("マスクで消去するのにかかる時間（秒）")]
    [SerializeField] private float eraseDuration = 0.5f;
    [Tooltip("消去が終わってから、次の表示までの待機時間（秒）")]
    [SerializeField] private float loopInterval = 2f;

    public async UniTask PlayLoopAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(startDelay), cancellationToken: token);

            while (!token.IsCancellationRequested)
            {
                wipeMask.ResetMask();
                targetImage.enabled = true;

                // 1. パラパラ漫画再生
                foreach (var frame in frames)
                {
                    targetImage.sprite = frame;
                    await UniTask.Delay(System.TimeSpan.FromSeconds(frameDuration), cancellationToken: token);
                }

                // ★追加：消し始める前に待機する
                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTimeBeforeErase), cancellationToken: token);

                // 2. マスクで消去
                await wipeMask.WipeOutAsync(eraseDuration, token);

                targetImage.enabled = false;

                // 3. 次のループまで待機
                await UniTask.Delay(System.TimeSpan.FromSeconds(loopInterval), cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException) { }
    }
    public void ResetToBlank()
    {
        wipeMask.ResetMask();
        targetImage.enabled = false;
    }
}