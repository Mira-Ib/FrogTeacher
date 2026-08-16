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

            // 1. 文字送りタスクと入力待ちタスクを生成
            var typewriterTask = typewriter.PlayTypewriterAsync(line, token);
            var inputTask = WaitForClickOrKeyAsync(token);

            // 2. どちらか早い方を待つ
            int completedIndex = await UniTask.WhenAny(typewriterTask, inputTask);

            // どちらの場合も喋り声SEは停止する
            AudioManager.Instance.StopLoopSE();

            if (completedIndex == 1)
            {
                // 【パターンA：文字送りの途中で入力が入った場合】
                typewriter.CompleteTypewriter();

                // 同一フレームでの連打誤動作を防ぐため1フレーム待機
                await UniTask.NextFrame(cancellationToken: token);

                // 次のセリフへ進むための入力を待つ
                await WaitForClickOrKeyAsync(token);
            }
            else
            {
                // 【パターンB：文字送りが最後まで自然に終わった場合】
                // ★修正: inputTask の再 await はエラーになるため、
                // 文字送りが終わった後に改めて新規で入力待ちを行う
                await WaitForClickOrKeyAsync(token);
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