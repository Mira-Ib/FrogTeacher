using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    // ★追加：暗転演出用のキャンバスグループ
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

    // --- 変更点：個別の AudioSource 参照を削除しました ---

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
        // ★追加：もう一度ボタンにイベントを登録
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(() => OnRetryClickedAsync().Forget());
        }
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(() => OnTitleClickedAsync().Forget());
        }
    }

    // ==========================================
    // ★追加：やり直しボタンを押した時の処理
    // ==========================================
    private async UniTaskVoid OnRetryClickedAsync()
    {
        // 1. 連打防止（ボタン全体を操作不可にする）
        buttonCanvasGroup.interactable = false;

        // 2. 音の演出（タイトル画面と全く同じ）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(SE.Chime);
            AudioManager.Instance.FadeOutBGM(0.5f);
        }

        // 3. 暗転演出（フェードアウト）
        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true; // 他の操作をブロック
            await blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        // 4. シーンの再読み込み
        // ※GameSessionData は静的（Static）なので、破棄されずそのまま残っています。
        // そのため、LectureManager は自動的に同じステージデータを読み込んで明転・開始します！
        await SceneManager.LoadSceneAsync("GameScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    // ==========================================
    // ★新規追加：タイトルに戻るボタンを押した時の処理
    // ==========================================
    private async UniTaskVoid OnTitleClickedAsync()
    {
        // 1. 連打防止
        buttonCanvasGroup.interactable = false;

        // 2. 音の演出（戻る・キャンセルのSEなどを指定）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutBGM(0.5f);
        }

        // 3. 暗転演出（フェードアウト）
        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        // 4. タイトルシーンをロード
        // ※シーン名は実際のBuild Settingsの登録名に合わせてください（"TitleScene" または "Title" など）
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

        // 1. スコア類の表示
        baseScoreText.text = $"{data.GettingScore}pt";
        yield return new WaitForSeconds(interval);

        penaltyText.color = Color.red;
        penaltyText.text = $"-{data.PenaltyCount * 30}pt";
        yield return new WaitForSeconds(interval);

        totalScoreText.text = $"{data.TotalScore}pt";
        yield return new WaitForSeconds(1.0f);

        rankText.text = data.Rank;
        yield return new WaitForSeconds(0.5f);

        // 2. カエル先生のコメント演出（AudioManagerによるループSE再生）
        yield return StartCoroutine(TypeTextWithSoundCoroutine(data.TeacherComment));

        // 3. 喋り終わった後にボタンを表示
        if (buttonGroupRect != null)
        {
            buttonGroupRect.DOAnchorPos(_originalButtonPos, 0.7f)
                .SetEase(Ease.OutBack, 0.7f)
                .OnComplete(() => {
                    buttonCanvasGroup.interactable = true;
                    buttonCanvasGroup.blocksRaycasts = true;
                });
        }
    }

    private void Update()
    {
        if (IsKeyboardInputDetected())
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                FocusDefaultButton();
        }
    }

    // ==================================================
    // ★リファクタリング：AudioManager のループ機能を使用
    // ==================================================
    private IEnumerator TypeTextWithSoundCoroutine(string text)
    {
        teacherCommentText.text = text;
        teacherCommentText.maxVisibleCharacters = 0;

        // 授業中と同じ SE（例：SE.Voice）をループ再生開始
        // ※SEの列挙型名はプロジェクトの定義に合わせてください
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoopSE(SE.Talking);
        }

        for (int i = 0; i <= text.Length; i++)
        {
            teacherCommentText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.05f);
        }

        // ループSEを停止
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoopSE();
        }
    }

    private void FocusDefaultButton()
    {
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
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