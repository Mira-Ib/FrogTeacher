using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class LectureManager : MonoBehaviour
{
    [Header("授業データ")]
    [SerializeField] private LectureData currentLectureData;

    [Header("各種コンポーネント")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private BubbleAnimView bubbleView;
    [SerializeField] private TeacherView teacherView;
    [SerializeField] private BlackboardView blackboardView;

    // ★追加：明転・暗転用の黒い画面
    [Header("演出用")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;
    [SerializeField] private CanvasGroup skipTextCanvasGroup;

    // --- ★追加：クイズパート連携用の変数 ---
    [Header("クイズパート連携")]
    [SerializeField] private GameObject quizRootObject;
    [SerializeField] private QuizDirector quizDirector;
    // ----------------------------------------


    private CancellationTokenSource _cts;

    private void Start()
    {
        StartLecture().Forget();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
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
            // ==========================================
            // ★追加1：タイトル画面からデータが渡ってきているか確認
            // ==========================================
            if (GameSessionData.SelectedStage != null && GameSessionData.SelectedStage.lectureData != null)
            {
                // 渡ってきていれば、インスペクタのデータを上書きする
                currentLectureData = GameSessionData.SelectedStage.lectureData;
                Debug.Log($"ステージ「{GameSessionData.SelectedStage.stageName}」の授業データを読み込みました！");
            }
            else
            {
                // データがない場合（GameSceneから直接再生した時など）は、インスペクタの default データをそのまま使う
                Debug.Log("GameSessionDataが空のため、インスペクタの授業データをそのまま再生します（テストプレイモード）");
            }

            // ==========================================
            // ★追加2：明転（フェードイン）演出
            // ==========================================
            if (blackoutCanvasGroup != null)
            {
                blackoutCanvasGroup.alpha = 1f; // 最初は真っ黒
                blackoutCanvasGroup.gameObject.SetActive(true);

                // チャイムの余韻を少し待ってから明転する
                await UniTask.Delay(500, cancellationToken: token);
                await blackoutCanvasGroup.DOFade(0f, 1.0f).WithCancellation(token);
                blackoutCanvasGroup.blocksRaycasts = false;
            }

            blackboardView.ClearBoard();

            // --- 1. 導入演出 ---
            // ※ここで鳴らしていたチャイムはタイトル画面で鳴らすようにしたので、コメントアウトか削除します
            // AudioManager.Instance.PlaySE(SE.Chime);
            await teacherView.EnterTeacherAsync(token);
            await UniTask.Delay(TimeSpan.FromSeconds(5.5f), cancellationToken: token);
            await bubbleView.ShowBubbleAsync(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
            AudioManager.Instance.PlayBGM(BGM.Lecture);

            // --- 2. 授業本編 ---
            foreach (var step in currentLectureData.steps)
            {
                teacherView.SetHandState(step.isFrogHandLowered);

                if (step.newBoardItems != null && step.newBoardItems.Length > 0)
                {
                    if (step.playChalkSound) AudioManager.Instance.PlaySE(SE.Chalk);
                    blackboardView.AddItems(step.newBoardItems);
                }

                await dialogueController.PlayDialogueStepAsync(step.dialogueText, token);
            }

            // --- 3. 終了演出 ---
            AudioManager.Instance.FadeOutBGM(1.0f);

            // ★修正ポイント：吹き出しと黒板を同時にフェードアウトさせる
            await UniTask.WhenAll(
                bubbleView.HideBubbleAsync(token),
                blackboardView.HideAndClearBoardAsync(token), // ここを追加！
                skipTextCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad).WithCancellation(token)
            );

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
        skipTextCanvasGroup.alpha = 0.0f;

        // ==========================================
        // ★追加：明転演出の強制完了（画面が暗いままになるバグを修正）
        // ==========================================
        if (blackoutCanvasGroup != null)
        {
            // 動いているフェードアニメーションがあれば強制停止
            blackoutCanvasGroup.DOKill();
            // 完全に透明にして、操作をブロックしないようにする
            blackoutCanvasGroup.alpha = 0f;
        }

        // ★修正：スキップ時は true を渡して強制的に即時停止させる
        AudioManager.Instance.StopLoopSE(true);
        AudioManager.Instance.StopBGMImmediate();
    }

    private void CompleteLecture()
    {
        Debug.Log("授業終了！クイズ本編へ移行します。");

        // ★追加：クイズパートへの遷移処理を開始
        TransitionToQuizAsync().Forget();
    }

    // --- ★追加：クイズパートへの遷移を管理する非同期メソッド ---
    private async UniTaskVoid TransitionToQuizAsync()
    {
        // 授業用のトークン(_cts)は破棄されているため、このManager自体の寿命に紐づく安全なトークンを使います
        var token = this.GetCancellationTokenOnDestroy();

        // 黒板が消えた後、少しだけ間を空ける（シーンの余韻）
        await UniTask.Delay(1000, cancellationToken: token);

        // クイズオブジェクトをアクティブにする（QuizDirectorのStart関数は消してある前提です）
        quizRootObject.SetActive(true);

        // クイズ側のUIViewerにイントロ演出をさせ、完了したらゲーム開始させる
        await quizDirector.BeginQuizPhaseAsync(token);
    }
}