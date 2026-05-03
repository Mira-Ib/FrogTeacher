using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DialogueController : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private TypewriterView typewriter;

    private bool _isWaitingForInput;

    /// <summary>
    /// Managerから呼ばれる、1ステップ（1文）分のセリフ表示処理
    /// </summary>
    public async UniTask PlayDialogueStepAsync(string line, CancellationToken token)
    {
        try
        {
            // 1. 文字送り開始と同時に喋りSE（ループ）を再生
            AudioManager.Instance.PlayLoopSE(SE.Talking);

            // 2. 文字送り再生
            await typewriter.PlayTypewriterAsync(line, token);
        }
        finally
        {
            // try-finallyブロックを使うことで、文字送りが「正常終了」しても
            // 「スキップで中断（例外発生）」しても、絶対にSEが止まるようにします。
            AudioManager.Instance.StopLoopSE();
        }

        // 3. 入力待ち（決定キー or クリック）
        await WaitForClickOrKeyAsync(token);
    }

    private async UniTask WaitForClickOrKeyAsync(CancellationToken token)
    {
        _isWaitingForInput = true;

        await UniTask.WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0),
            cancellationToken: token);

        _isWaitingForInput = false;
    }

    public void FastForward()
    {
        typewriter.FastForward();
        _isWaitingForInput = false;
    }
}