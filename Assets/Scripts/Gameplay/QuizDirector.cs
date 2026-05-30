using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System; // ★追加：ActionやExceptionを使うために必要です

public struct QuizResultData
{
    public bool isCorrect;
    public int comboCount;
    public int multiplier;
    public int totalScore;
}


public class QuizDirector : MonoBehaviour
{
    [SerializeField] private QuestionLibrary questionLibrary;
    [SerializeField] private QuizUIViewer uiViewer;
    [SerializeField] private ResultView resultView;

    private Queue<QuestionData> currentSequence = new Queue<QuestionData>();
    private int totalAnsweredCount = 0;
    private int penaltyCount = 0;
    private int gettingScore = 0;
    private bool isFirstLoop = true;

    [Header("Components")]
    [SerializeField] private SpeedGaugeView gaugeView;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private GameObject quiz;
    [SerializeField] private GameObject result;
    [SerializeField] private GameObject frogTeacher;

    [Header("Settings")]
    [SerializeField] private float bonusTimeLimit = 5.0f;

    [SerializeField] private TeacherView teacherView;

    private float elapsedBonusTime;
    private bool isQuizActive = false;
    private bool isGameEnded = false;

    // ★改善2：staticのまま安全に使うため、eventキーワードを追加
    public static event Action<QuizResultData> OnCorrect;
    public static event Action OnWrong;

    // 現在の問題データを保持
    private QuestionData currentQuestionData;

    // ★追加：外部から「現在の問題」を読み取れるようにする（読み取り専用なので安全です）
    public QuestionData CurrentQuestionData => currentQuestionData;

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

    public async UniTask BeginQuizPhaseAsync(CancellationToken token)
    {
        // ★改善4：共有スペース（GameSessionData）からクイズデータを取り出し、Libraryに注入する
        QuizData targetQuizData = null;
        if (GameSessionData.SelectedStage != null)
        {
            targetQuizData = GameSessionData.SelectedStage.quizData;
        }
        questionLibrary.Initialize(targetQuizData);

        StartNewLoop();
        currentQuestionData = currentSequence.Dequeue();

        await uiViewer.PlayIntroSequenceAsync(currentQuestionData, token);
        await UniTask.Delay(400, cancellationToken: token);

        if (teacherView != null)
        {
            teacherView.SwitchToQuizMode();
        }

        totalAnsweredCount++;
        isQuizActive = true;
        elapsedBonusTime = 0f;

        AudioManager.Instance.PlayBGM(BGM.Quiz, false);
    }

    private void Update()
    {
        if (!isQuizActive || isGameEnded) return;

        elapsedBonusTime += Time.deltaTime;
        float ratio = Mathf.Max(0, 1.0f - (elapsedBonusTime / bonusTimeLimit));
        gaugeView.UpdateVisual(ratio);
    }

    // ==================================================
    // ★修正：インスペクタのButtonから直接呼ばれる用のメソッド（voidに戻す）
    // ==================================================
    public void OnAnswerSubmitted(bool playerChoice)
    {
        // 実際の非同期処理を呼び出して、Forget() で投げっ放しにする
        ProcessAnswerAsync(playerChoice).Forget();
    }

    // ==================================================
    // 実際の処理を行う非同期メソッド（中身は先ほどと同じです）
    // ==================================================
    private async UniTaskVoid ProcessAnswerAsync(bool playerChoice)
    {
        if (!isQuizActive || isGameEnded) return;
        isQuizActive = false;

        bool isCorrect = (playerChoice == currentQuestionData.isTrueKero);

        if (isCorrect)
        {
            float ratio = Mathf.Max(0, 1.0f - (elapsedBonusTime / bonusTimeLimit));

            QuizResultData resultData = new QuizResultData();
            resultData.isCorrect = true;
            resultData.comboCount = scoreCalculator.GetCurrentComboCount() + 1;
            resultData.multiplier = scoreCalculator.GetCurrentMultiplier(ratio);
            resultData.totalScore = scoreCalculator.CalculateScore(ratio);

            // クラス内でイベントを呼ぶ際はそのまま Invoke でOKです
            OnCorrect?.Invoke(resultData);
            gettingScore += resultData.totalScore;

            Debug.Log($"<color=cyan>正解！</color> {resultData.totalScore}点");
        }
        else
        {
            scoreCalculator.ResetCombo();
            penaltyCount += 1;

            OnWrong?.Invoke();

            Debug.Log("<color=red>不正解...</color>");
        }

        if (!isGameEnded)
        {
            try
            {
                // 次の問題へ進む前に0.5秒待機（シーン破棄時は自動キャンセル）
                await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());

                // 待機中にゲーム終了フラグが立っていなければ次の問題へ
                if (!isGameEnded)
                {
                    ShowNextQuestion();
                }
            }
            catch (System.OperationCanceledException)
            {
                // シーン遷移などでキャンセルされた場合は何もしない
            }
        }
    }

    private void StartNewLoop()
    {
        List<QuestionData> flow;

        if (isFirstLoop)
        {
            flow = questionLibrary.GenerateFirstLoopSequence();
            isFirstLoop = false;
        }
        else
        {
            flow = questionLibrary.GenerateRandomSequence();
        }

        currentSequence = new Queue<QuestionData>(flow);
    }

    public void ShowNextQuestion()
    {
        if (isGameEnded) return;
        if (currentSequence.Count == 0) StartNewLoop();

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
        isGameEnded = true;
        isQuizActive = false;
        GameEndAsync().Forget();
    }

    // ★改善3：シーン破棄時のエラー（MissingReferenceException）防止
    private async UniTaskVoid GameEndAsync()
    {
        try
        {
            await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());

            quiz.SetActive(false);
            frogTeacher.SetActive(false);
            result.SetActive(true);
            resultView.ShowResult(gettingScore, penaltyCount);
        }
        catch (OperationCanceledException)
        {
            // シーン遷移などでキャンセルされた場合は何もしない
        }
    }

    // ★追加：禁忌肢などの演出時に、外部からクイズの進行を強制的にストップさせるため
    public void ForceStopQuiz()
    {
        isGameEnded = true;
        isQuizActive = false;
    }
}