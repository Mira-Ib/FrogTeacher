using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public struct QuizResultData
{
    public bool isCorrect;
    public int comboCount;    // ★追加：現在のコンボ数
    public int multiplier;
    public int totalScore;
}

public class QuizDirector : MonoBehaviour
{
    [SerializeField] private QuestionLibrary questionLibrary;
    [SerializeField] private QuizUIViewer uiViewer; // UI反映担当
    [SerializeField] private ResultView resultView;

    private Queue<QuestionData> currentSequence = new Queue<QuestionData>();
    private int totalAnsweredCount = 0; // 通算回答数
    private int penaltyCount = 0;
    private int gettingScore = 0;
    private bool isFirstLoop = true;

    [Header("Components")]
    [SerializeField] private SpeedGaugeView gaugeView;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private GameObject quiz;
    [SerializeField] private GameObject result;

    [Header("Settings")]
    [SerializeField] private float bonusTimeLimit = 5.0f; // ボーナスが減りきるまでの時間

    // ★追加：TeacherViewへの参照（インスペクタからアタッチしてください）
    [SerializeField] private TeacherView teacherView;

    private float elapsedBonusTime; // 経過時間
    private bool isQuizActive = false;

    // ★追加: ゲームが完全に終了したかどうかを判定するフラグ
    private bool isGameEnded = false;

    // --- 追加：演出用のイベント ---
    public static System.Action<QuizResultData> OnCorrect; // 正解した時（データを渡す）
    public static System.Action OnWrong;                   // 不正解の時
    void OnEnable()
    {
        MusicTimer.OnGameProgressed += ShuffleQueue;
        MusicTimer.OnGameEnded += GameEnd;
    }

    void OnDisable()
    {
        MusicTimer.OnGameProgressed -= ShuffleQueue;
        MusicTimer.OnGameEnded -= GameEnd;
    }

    /// <summary>
    /// ★追加：外部（LectureManager等）から呼ばれる、クイズパートの開始地点
    /// </summary>
    public async UniTask BeginQuizPhaseAsync(CancellationToken token)
    {
        StartNewLoop();
        currentQuestionData = currentSequence.Dequeue();

        // 1. UIのフェードイン演出（ボタン表示まで）を待つ
        await uiViewer.PlayIntroSequenceAsync(currentQuestionData, token);

        // 2. ★要望：UI表示後、少し待機する
        await UniTask.Delay(400, cancellationToken: token);

        // 3. ★要望：この瞬間にカエル先生をクイズモード（ダンス）に切り替える
        if (teacherView != null)
        {
            teacherView.SwitchToQuizMode();
        }

        // 4. ゲーム本編開始！
        totalAnsweredCount++;
        isQuizActive = true;
        elapsedBonusTime = 0f;

        AudioManager.Instance.PlayBGM(BGM.Quiz, false);
    }

    private void Update()
    {
        if (!isQuizActive || isGameEnded) return;

        // 時間は進めるが、0以下になってもクイズは終了させない
        elapsedBonusTime += Time.deltaTime;

        // ゲージ表示用の割合を計算 (1.0から始まり、0.0で止まる)
        float ratio = Mathf.Max(0, 1.0f - (elapsedBonusTime / bonusTimeLimit));

        // Viewに更新を依頼（0.0になったら赤も消えて背景だけになる）
        gaugeView.UpdateVisual(ratio);
    }

    // プレイヤーが回答したときに呼ぶ
    public void OnAnswerSubmitted(bool playerChoice)
    {
        // ★修正: ゲーム終了状態なら何もしない（受付拒否）
        if (!isQuizActive || isGameEnded) return;
        isQuizActive = false;

        // 1. 正誤判定（ロジック）
        bool isCorrect = (playerChoice == currentQuestionData.isTrueKero);

        if (isCorrect)
        {
            // --- 正解時の処理 ---
            float ratio = Mathf.Max(0, 1.0f - (elapsedBonusTime / bonusTimeLimit));

            // 演出用のデータを生成して計算結果を詰める
            QuizResultData result = new QuizResultData();
            result.isCorrect = true;
            result.comboCount = scoreCalculator.GetCurrentComboCount() + 1;
            result.multiplier = scoreCalculator.GetCurrentMultiplier(ratio);
            result.totalScore = scoreCalculator.CalculateScore(ratio);

            // ★演出イベント発行（正解用）
            OnCorrect?.Invoke(result);
            gettingScore += result.totalScore;

            Debug.Log($"<color=cyan>正解！</color> {result.totalScore}点");
        }
        else
        {
            // --- 不正解時の処理 ---
            scoreCalculator.ResetCombo();
            penaltyCount += 1;

            // ★演出イベント発行（不正解用）
            OnWrong?.Invoke();

            Debug.Log("<color=red>不正解...</color>");
        }

        // ★修正: ゲーム終了フラグが立っていない場合のみ、次の問題を予約する
        if (!isGameEnded)
        {
            Invoke(nameof(ShowNextQuestion), 0.5f);
        }
    }

    // 新しい周回（100問単位）を開始する
    private void StartNewLoop()
    {
        List<QuestionData> flow;

        if (isFirstLoop)
        {
            flow = questionLibrary.GenerateFirstLoopSequence();
            isFirstLoop = false; // 次回からは完全ランダム
        }
        else
        {
            flow = questionLibrary.GenerateRandomSequence();
        }

        // Queueに変換して管理しやすくする
        currentSequence = new Queue<QuestionData>(flow);
    }

    // 現在表示中の問題データを保持する変数（追加）
    private QuestionData currentQuestionData;

    // 次の問題を出す処理（一部修正）
    public void ShowNextQuestion()
    {
        if (isGameEnded) return;
        if (currentSequence.Count == 0) StartNewLoop();

        // 現在の問題を保持しておく
        currentQuestionData = currentSequence.Dequeue();
        uiViewer.SetupQuestionUI(currentQuestionData, totalAnsweredCount);
        totalAnsweredCount++;
        isQuizActive = true;
        elapsedBonusTime = 0f;
    }

    private void ShuffleQueue()
    {
        List<QuestionData> list = new List<QuestionData>(currentSequence);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        currentSequence.Clear();

        foreach (var item in list)
        {
            currentSequence.Enqueue(item);
        }
    }

    private void GameEnd()
    {
        // ★追加: 音楽終了イベント等で呼ばれたら、すぐにフラグを立てる
        isGameEnded = true;

        // 即座にクイズ進行（ゲージ減少など）を止める
        isQuizActive = false;

        GameEndAsync().Forget();
    }

    private async UniTask GameEndAsync()
    {
        await UniTask.Delay(1000);
        quiz.SetActive(false);
        result.SetActive(true);
        resultView.ShowResult(gettingScore, penaltyCount);
    }
}