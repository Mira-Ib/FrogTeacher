using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using unityroom.Api;

public class ResultView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI baseScoreText;
    [SerializeField] private TextMeshProUGUI penaltyText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI teacherCommentText;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private RectTransform buttonGroupRect;
    private Vector2 _originalButtonPos;

    [Header("Transition")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

    private void Start()
    {
        ClearUI();

        if (buttonGroupRect != null)
        {
            _originalButtonPos = buttonGroupRect.anchoredPosition;
            buttonGroupRect.anchoredPosition = _originalButtonPos + new Vector2(0, -800f);

            buttonCanvasGroup.interactable = false;
            buttonCanvasGroup.blocksRaycasts = false;
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(() => OnRetryClickedAsync().Forget());
        }
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(() => OnTitleClickedAsync().Forget());
        }
    }

    private async UniTaskVoid OnRetryClickedAsync()
    {
        buttonCanvasGroup.interactable = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(SE.Chime);
            AudioManager.Instance.FadeOutBGM(0.5f);
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
        buttonCanvasGroup.interactable = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutBGM(0.5f);
        }

        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        await SceneManager.LoadSceneAsync("TitleScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public void ShowResult(int rawScore, int penalties)
    {
        var data = ResultProcessor.Process(rawScore, penalties);
        StartCoroutine(ResultFlowCoroutine(data));
    }

    private IEnumerator ResultFlowCoroutine(ResultDisplayData data)
    {
        const float interval = 0.5f;

        yield return null;

        baseScoreText.text = $"{data.GettingScore}pt";
        yield return new WaitForSeconds(interval);

        penaltyText.color = Color.red;
        penaltyText.text = $"-{data.PenaltyCount * 30}pt";
        yield return new WaitForSeconds(interval);

        totalScoreText.text = $"{data.TotalScore}pt";
        yield return new WaitForSeconds(1.0f);

        rankText.text = data.Rank;
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeTextWithSoundCoroutine(data.TeacherComment));

        if (buttonGroupRect != null)
        {
            buttonGroupRect.DOAnchorPos(_originalButtonPos, 0.7f)
                .SetEase(Ease.OutBack, 0.7f)
                .OnComplete(() => {
                    buttonCanvasGroup.interactable = true;
                    buttonCanvasGroup.blocksRaycasts = true;

                    // ★追加：演出完了直後に初期フォーカスを設定
                    FocusDefaultButton();
                });
        }
        // ==========================================
        // ★追加：ここでポーズ使用履歴を判定し、ランキング処理を分岐させます
        // ==========================================
        if (!GameSessionData.HasUsedPause)
        {
            UnityroomApiClient.Instance.SendScore(1, data.TotalScore, ScoreboardWriteMode.HighScoreDesc);
            Debug.Log("<color=green>ポーズ未使用のため、ランキングにスコアを登録します！</color>");
        }
        else
        {
            // ポーズ使用時は送信処理を行いません
            Debug.Log("<color=yellow>ポーズが使用されたため、ランキング登録の対象外です。</color>");
        }
    }

    private void Update()
    {
        // 入力があった際に何も選択されていなければフォーカスを戻す
        if (IsKeyboardInputDetected())
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                // ボタンが表示（操作可能）な状態の時のみフォーカスする
                if (buttonCanvasGroup.interactable)
                {
                    FocusDefaultButton();
                }
            }
        }
    }

    private IEnumerator TypeTextWithSoundCoroutine(string text)
    {
        teacherCommentText.text = text;
        teacherCommentText.maxVisibleCharacters = 0;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoopSE(SE.Talking);
        }

        for (int i = 0; i <= text.Length; i++)
        {
            teacherCommentText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.05f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoopSE();
        }
    }

    private void FocusDefaultButton()
    {
        if (EventSystem.current != null && retryButton != null)
        {
            // 選択状態を確実に反映させるため、一度クリアしてからセットする
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }

    private void ClearUI()
    {
        baseScoreText.text = "";
        penaltyText.text = "";
        totalScoreText.text = "";
        rankText.text = "";
        teacherCommentText.text = "";
    }

    private bool IsKeyboardInputDetected()
    {
        return Input.GetAxisRaw("Horizontal") != 0 ||
               Input.GetAxisRaw("Vertical") != 0 ||
               Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return);
    }
}