using TMP_Ruby;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class QuizUIViewer : MonoBehaviour
{
    [Header("UIフェード用")]
    [Tooltip("吹き出し全体の透明度を制御するCanvasGroup")]
    [SerializeField] private CanvasGroup bubbleCanvasGroup;
    [Header("追加UIのフェード用CanvasGroup")]
    [SerializeField] private CanvasGroup speedGaugeCanvasGroup;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private CanvasGroup optionButtonCanvasGroup;
    [SerializeField] private CanvasGroup timerGaugeCanvasGroup;

    [Header("ルビ対応メインテキスト")]
    [SerializeField] private TextMeshProRuby rubyInput;

    [Header("前後定型文（通常のTMP）")]
    [SerializeField] private TextMeshProUGUI introTextDisplay;
    [SerializeField] private TextMeshProUGUI outroTextDisplay;

    // メインテキストの実体（アルファ値操作用）
    private TMP_Text _mainTextComponent;

    private void Awake()
    {
        _mainTextComponent = rubyInput.GetComponent<TMP_Text>();
    }

    /// <summary>
    /// ★追加：クイズ開始時の「なるほど！」から始まる一連のフェードイン演出
    /// </summary>
    public async UniTask PlayIntroSequenceAsync(QuestionData firstQuestion, CancellationToken token)
    {
        // 1. 全てを透明に初期化
        bubbleCanvasGroup.alpha = 0f;
        introTextDisplay.alpha = 0f;
        outroTextDisplay.alpha = 0f;
        _mainTextComponent.alpha = 0f;

        // 2. 「なるほど！」をセット
        rubyInput.Text = "なるほど！";
        await UniTask.Delay(100, cancellationToken: token);

        // 3. 吹き出し本体と「なるほど！」を同時にフェードイン
        await DOTween.Sequence()
            .Join(bubbleCanvasGroup.DOFade(1f, 0.5f))
            .Join(_mainTextComponent.DOFade(1f, 0.5f))
            .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);

        // 少し余韻（「なるほど！」を読ませる時間）
        await UniTask.Delay(800, cancellationToken: token);

        // 4. 「なるほど！」をフェードアウト
        await _mainTextComponent.DOFade(0f, 0.3f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);

        // 5. 最初の問題テキストをセット（まだ透明）
        introTextDisplay.text = "つまり…";
        rubyInput.Text = firstQuestion.questionText; // 「たし算はじゃんけんと同じ」など
        outroTextDisplay.text = "ということですか！？";

        // フォントサイズの設定
        _mainTextComponent.enableAutoSizing = firstQuestion.useAutoSizing;
        if (!firstQuestion.useAutoSizing) _mainTextComponent.fontSize = firstQuestion.fontSize;

        // 6. 順番にフェードイン（時間差）
        await introTextDisplay.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
        await UniTask.Delay(400, cancellationToken: token); // 間をとる

        await _mainTextComponent.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
        await UniTask.Delay(400, cancellationToken: token); // 間をとる

        await outroTextDisplay.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
        await UniTask.Delay(100, cancellationToken: token); // 間をとる

        // ★追加：テキストが全て出終わった後、ゲージとボタンをフェードイン
        optionButtonCanvasGroup.interactable = true;
        await UniTask.WhenAll(
            speedGaugeCanvasGroup.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token),
            buttonsCanvasGroup.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token),
            optionButtonCanvasGroup.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token),
            timerGaugeCanvasGroup.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token)
        );

        // 全て表示完了！
    }

    /// <summary>
    /// 2問目以降の通常のセットアップ（即時表示）
    /// </summary>
    public void SetupQuestionUI(QuestionData data, int answeredCount)
    {
        // 即座に不透明にする
        bubbleCanvasGroup.alpha = 1f;
        introTextDisplay.alpha = 1f;
        outroTextDisplay.alpha = 1f;
        _mainTextComponent.alpha = 1f;

        introTextDisplay.text = (answeredCount == 0) ? "つまり…" : "じゃあ…";
        outroTextDisplay.text = "ということですか！？";

        _mainTextComponent.enableAutoSizing = data.useAutoSizing;
        if (!data.useAutoSizing)
        {
            _mainTextComponent.fontSize = data.fontSize;
        }

        rubyInput.Text = data.questionText;
    }

    private void OnEnable() { MusicTimer.OnGameEnded += ShowTimeUp; }
    private void OnDisable() { MusicTimer.OnGameEnded -= ShowTimeUp; }

    public void ShowTimeUp()
    {
        introTextDisplay.text = "";
        outroTextDisplay.text = "";
        rubyInput.Text = "時間だ！";
    }

    // ==========================================
    // ★追加：禁忌肢を踏んだ時にメインテキストを書き換える
    // ==========================================
    public void ShowTabooMessage()
    {
        introTextDisplay.text = "";
        outroTextDisplay.text = "";
        rubyInput.Text = "ひどい！";
    }

    // ==========================================
    // ★追加：禁忌肢ゲームオーバー時に、回答ボタンの操作を完全にロックする
    // ==========================================
    public void DisableAllAnswerButtons()
    {
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;
        }
        if (optionButtonCanvasGroup != null)
        {
            optionButtonCanvasGroup.interactable = false;
            optionButtonCanvasGroup.blocksRaycasts = false;
        }
    }
}