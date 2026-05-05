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
            await typewriter.PlayTypewriterAsync(line, token);
        }
        finally
        {
            // ここは引数なしでOK（自然に鳴り終わる）
            AudioManager.Instance.StopLoopSE();
        }

        await WaitForClickOrKeyAsync(token);
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