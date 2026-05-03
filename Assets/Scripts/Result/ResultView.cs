using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private RectTransform buttonGroupRect; // 追加：ボタン親のRectTransform
    private Vector2 _originalButtonPos; // ボタンの本来の（インスペクターで設定した）位置を保存用

    [Header("Audio")]
    [SerializeField] private AudioSource voiceAudioSource; // ループ設定にしたAudioSource

    private void Start()
    {
        // 初期状態は非表示、または空文字
        ClearUI();

        if (buttonGroupRect != null)
        {
            // 本来の位置（インスペクターで設定したゴール地点）を記録
            _originalButtonPos = buttonGroupRect.anchoredPosition;

            // 変更点：透明度は操作せず、位置だけを「画面の下の外側」へ飛ばす
            // -800f は一般的な解像度で画面外に出る目安ですが、UIの構造に合わせて調整してください
            buttonGroupRect.anchoredPosition = _originalButtonPos + new Vector2(0, -800f);

            // 動いている最中にクリックされないよう、操作不可にはしておく
            buttonCanvasGroup.interactable = false;
            buttonCanvasGroup.blocksRaycasts = false;
        }
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

        // 2. カエル先生のコメント演出（SE付き）
        yield return StartCoroutine(TypeTextWithSoundCoroutine(data.TeacherComment));

        // 3. 喋り終わった後にボタンを表示
        if (buttonGroupRect != null)
        {
            // DOAnchorPos（移動）のみを実行
            // 時間を少し長め（0.7s程度）にすると、長い距離を移動する勢いが出ます
            buttonGroupRect.DOAnchorPos(_originalButtonPos, 0.7f)
                .SetEase(Ease.OutBack, 0.7f) // 「シュバッ」と出て「ピタッ」と止まる演出
                .OnComplete(() => {
                    // 到着したら操作可能にする
                    buttonCanvasGroup.interactable = true;
                    buttonCanvasGroup.blocksRaycasts = true;
                });
        }
    }

    private void Update()
    {
        if (IsKeyboardInputDetected())
        {
            if ((EventSystem.current.currentSelectedGameObject == null))
            FocusDefaultButton();
        }
    }

    private IEnumerator TypeTextWithSoundCoroutine(string text)
    {
        teacherCommentText.text = text;
        teacherCommentText.maxVisibleCharacters = 0;

        // SE再生開始（Inspector側で Loop をオンにしておいてください）
        if (voiceAudioSource != null)
        {
            voiceAudioSource.Play();
        }

        for (int i = 0; i <= text.Length; i++)
        {
            teacherCommentText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.05f);
        }

        // SE停止
        if (voiceAudioSource != null)
        {
            voiceAudioSource.Stop();
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
        // 矢印キー、WASD、Enter、Spaceのいずれかが押されたか
        return Input.GetAxisRaw("Horizontal") != 0 ||
               Input.GetAxisRaw("Vertical") != 0 ||
               Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return);
    }
}