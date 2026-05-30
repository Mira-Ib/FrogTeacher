using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class TabooGameOverHandler : MonoBehaviour
{
    [Header("システム参照")]
    [SerializeField] private QuizDirector quizDirector;
    // ★追加：メインテキストを書き換えるために参照します
    [SerializeField] private QuizUIViewer quizUIViewer;

    [Header("ゲームオーバーパネル（上から降ってくるUI）")]
    [SerializeField] private RectTransform gameOverPanelRect;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private float panelDropDuration = 0.6f;

    [Header("画面暗転用（リトライ・タイトル移動時）")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

    private Vector2 _panelOriginalPos;

    private void Awake()
    {
        // 1. ゲームオーバーパネルを画面の外（上空）へ飛ばしておく
        if (gameOverPanelRect != null)
        {
            _panelOriginalPos = gameOverPanelRect.anchoredPosition;
            gameOverPanelRect.anchoredPosition = _panelOriginalPos + new Vector2(0f, 1200f);
        }

        // 2. パネル内のボタンを最初は触れないようにしておく
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        QuizDirector.OnWrong += HandleWrongAnswer;

        if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClickedAsync().Forget());
        if (titleButton != null) titleButton.onClick.AddListener(() => OnTitleClickedAsync().Forget());
    }

    private void OnDisable()
    {
        QuizDirector.OnWrong -= HandleWrongAnswer;
    }

    private void HandleWrongAnswer()
    {
        if (quizDirector == null || quizDirector.CurrentQuestionData == null) return;

        if (quizDirector.CurrentQuestionData.questionType == QuestionType.Taboo)
        {
            quizDirector.ForceStopQuiz();
            PlayTabooGameOverSequenceAsync().Forget();
        }
    }

    private async UniTaskVoid PlayTabooGameOverSequenceAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        // 1. BGMのフェードアウト
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutBGM(0.4f);
        }

        // ==========================================
        // 2. ★修正：別UIを出すのではなく、メインテキストを「ひどい！」に書き換える
        // ==========================================
        if (quizUIViewer != null)
        {
            quizUIViewer.ShowTabooMessage();
        }

        // 3. 指定の0.5秒待機（テキストを読ませる時間）
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f), cancellationToken: token);

        // 4. ゲームオーバーパネルが降ってくる演出
        if (gameOverPanelRect != null)
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }

            await gameOverPanelRect.DOAnchorPos(_panelOriginalPos, panelDropDuration)
                .SetEase(Ease.OutBounce)
                .ToUniTask(cancellationToken: token);

            FocusDefaultButton();
        }
    }

    private void FocusDefaultButton()
    {
        if (EventSystem.current != null && retryButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }

    private async UniTaskVoid OnRetryClickedAsync()
    {
        if (panelCanvasGroup != null) panelCanvasGroup.interactable = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(SE.Chime);
        }

        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        await SceneManager.LoadSceneAsync("GameScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    private async UniTaskVoid OnTitleClickedAsync()
    {
        if (panelCanvasGroup != null) panelCanvasGroup.interactable = false;

        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        await SceneManager.LoadSceneAsync("TitleScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}