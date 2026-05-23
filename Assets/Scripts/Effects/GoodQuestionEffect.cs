using UnityEngine;
using DG.Tweening;

/// <summary>
/// 良い質問（GoodQuestion）に正解した際、生徒の声と同じアニメーションでUIを表示するクラス
/// </summary>
public class GoodQuestionEffect : MonoBehaviour
{
    [Header("参照")]
    [SerializeField, Tooltip("現在の問題データを読み取るためのQuizDirector")]
    private QuizDirector quizDirector;

    [SerializeField, Tooltip("「良い質問ケロ！」のテキストを含むCanvasGroup")]
    private CanvasGroup textCanvasGroup;

    [Header("Appearance (出現)")]
    [SerializeField] private float targetScale = 1.0f;
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    [Header("Wait (静止)")]
    [SerializeField] private float displayDuration = 1.0f;

    [Header("Fade Out (消滅)")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Tween _sequenceTween;

    private void Awake()
    {
        // 初期状態のセット（透明 ＆ 大きさゼロ）
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 0f;
            textCanvasGroup.blocksRaycasts = false;
            textCanvasGroup.transform.localScale = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        QuizDirector.OnCorrect += HandleCorrectAnswer;
    }

    private void OnDisable()
    {
        QuizDirector.OnCorrect -= HandleCorrectAnswer;
        _sequenceTween?.Kill(); // 破棄時にアニメーションも止める
    }

    private void HandleCorrectAnswer(QuizResultData data)
    {
        if (quizDirector == null || quizDirector.CurrentQuestionData == null) return;

        if (quizDirector.CurrentQuestionData.questionType == QuestionType.GoodQuestion)
        {
            PlayEffectSequence();
        }
    }

    private void PlayEffectSequence()
    {
        if (textCanvasGroup == null) return;

        // 連続で呼ばれた場合、前のアニメーションを強制終了してリセット
        _sequenceTween?.Kill();

        AudioManager.Instance.PlaySE(SE.Applause);

        // 1. 初期状態のセット（大きさ0、不透明度1）
        textCanvasGroup.transform.localScale = Vector3.zero;
        textCanvasGroup.alpha = 1f;

        // 2. DOTweenのシーケンス作成（StudentVoiceEffectと全く同じ構造）
        Sequence seq = DOTween.Sequence();

        // 【出現】ぱきっと拡大して、規定の大きさ（targetScale）へ
        seq.Append(textCanvasGroup.transform.DOScale(targetScale, popDuration).SetEase(popEase));

        // 【静止】指定した秒数だけ待つ
        seq.AppendInterval(displayDuration);

        // 【消滅】フェードアウト
        seq.Append(textCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

        // 生成したシーケンスを変数に保持（連続再生時のリセット用）
        _sequenceTween = seq;
    }
}