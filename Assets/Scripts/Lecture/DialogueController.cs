using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DialogueController : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private TypewriterView typewriter;

    /// <summary>
    /// Managerから呼ばれる、1ステップ（1文）分のセリフ表示処理
    /// </summary>
    public async UniTask PlayDialogueStepAsync(string line, CancellationToken token)
    {
        try
        {
            AudioManager.Instance.PlayLoopSE(SE.Talking);

            var typewriterTask = typewriter.PlayTypewriterAsync(line, token);
            var inputTask = WaitForClickOrKeyAsync(token);

            int completedIndex = await UniTask.WhenAny(typewriterTask, inputTask);

            if (completedIndex == 1)
            {
                // ★ FastForward() ではなく、1文スキップ専用の CompleteTypewriter() を呼ぶ
                typewriter.CompleteTypewriter();

                AudioManager.Instance.StopLoopSE();

                // 同一フレームでの連打誤動作を防ぐため1フレーム待機
                await UniTask.NextFrame(cancellationToken: token);

                // 改めて「次のセリフに進む」ための入力を待つ
                await WaitForClickOrKeyAsync(token);
            }
            else
            {
                AudioManager.Instance.StopLoopSE();
                await inputTask;
            }
        }
        finally
        {
            AudioManager.Instance.StopLoopSE();
        }
    }

    private async UniTask WaitForClickOrKeyAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0),
            cancellationToken: token);
    }

    public void FastForward()
    {
        typewriter.FastForward();
    }
}