using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class LectureManager : MonoBehaviour
{
    [Header("授業データ")]
    [SerializeField] private LectureData currentLectureData;

    [Header("各種コンポーネント")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private BubbleAnimView bubbleView;
    [SerializeField] private TeacherView teacherView;
    [SerializeField] private BlackboardView blackboardView;

    // ※ [SerializeField] private LectureAudioManager audioManager; は削除しました

    private CancellationTokenSource _cts;

    private void Start()
    {
        StartLecture().Forget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SkipLecture();
        }
    }

    private async UniTaskVoid StartLecture()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            blackboardView.ClearBoard();

            // --- 1. 導入演出 ---
            AudioManager.Instance.PlaySE(SE.Chime);
            await teacherView.EnterTeacherAsync(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: token);
            await bubbleView.ShowBubbleAsync(token);
            AudioManager.Instance.PlayBGM(BGM.Lecture);

            // --- 2. 授業本編 ---
            foreach (var step in currentLectureData.steps)
            {
                teacherView.SetHandState(step.isFrogHandRaised);

                if (step.newBoardItems != null && step.newBoardItems.Length > 0)
                {
                    if (step.playChalkSound) AudioManager.Instance.PlaySE(SE.Chalk);
                    blackboardView.AddItems(step.newBoardItems); // アニメーションなし即時表示
                }

                // セリフの表示
                await dialogueController.PlayDialogueStepAsync(step.dialogueText, token);
            }

            // --- 3. 終了演出 ---
            await bubbleView.HideBubbleAsync(token);

            AudioManager.Instance.FadeOutBGM(1.0f);
            CompleteLecture();
        }
        catch (OperationCanceledException)
        {
            ExecuteSkipFastForward();
            CompleteLecture();
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void SkipLecture()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    private void ExecuteSkipFastForward()
    {
        Debug.Log("授業演出をスキップ！最終状態へ移行します。");
        teacherView.FastForward();
        bubbleView.FastForward();
        blackboardView.FastForward();
        dialogueController.FastForward();

        // AudioManagerの強制停止処理（ILecturePlayableの代わり）
        AudioManager.Instance.StopLoopSE();
        AudioManager.Instance.StopBGMImmediate();
    }

    private void CompleteLecture()
    {
        Debug.Log("授業終了！クイズ本編へ移行！");
    }
}