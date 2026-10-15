using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseView : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private PauseController pauseController;

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton; // 「ゲームに戻る」ボタン（一番上）
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private AudioSource pauseAudioSource;

    [Header("Transition")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

    private void Start()
    {
        // Controllerのイベントを購読
        if (pauseController != null)
        {
            pauseController.OnPauseToggled += HandlePauseUI;
        }

        if (pauseAudioSource != null)
        {
            // これがインスペクターの「チェックボックス」と同じ役割をします
            pauseAudioSource.ignoreListenerPause = true;
        }
        // 「はじめからやり直す」ボタンに登録
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => OnRestartClickedAsync().Forget());
        }
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(() => OnTitleClickedAsync().Forget());
        }
    }

    private async UniTaskVoid OnRestartClickedAsync()
    {
        // 1. ポーズ画面のボタンを連打できないようにする
        restartButton.interactable = false;

        // 2. タイムスケールを戻す（ポーズ中で Time.timeScale = 0 になっている場合必須！）
        Time.timeScale = 1f;
        // ★これを追加：AudioListenerのポーズ状態を強制解除する（耳栓を外す）
        AudioListener.pause = false;

        // 3. 音の演出
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(SE.Chime);
            AudioManager.Instance.FadeOutBGM(0.5f);
        }

        // 4. 暗転演出
        if (blackoutCanvasGroup != null)
        {
            // DOTweenはTimeScaleの影響を受けるため、SetUpdate(true)をつけて確実に動かす
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f)
                .SetUpdate(true) // ★重要：ポーズ中でもアニメーションさせる
                .WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        // 5. シーン再読み込み
        await SceneManager.LoadSceneAsync("GameScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    // ==========================================
    // ★新規追加：タイトルに戻るボタンを押した時の処理
    // ==========================================
    private async UniTaskVoid OnTitleClickedAsync()
    {
        // 1. 連打防止
        if (restartButton != null) restartButton.interactable = false;
        if (titleButton != null) titleButton.interactable = false;

        // 2. ポーズ状態の強制解除（超重要！）
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 3. 音の演出
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(SE.Back);
            AudioManager.Instance.FadeOutBGM(0.5f);
        }

        // 4. 暗転演出
        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.gameObject.SetActive(true);
            blackoutCanvasGroup.blocksRaycasts = true;
            await blackoutCanvasGroup.DOFade(1f, 1.0f)
                .SetUpdate(true) // DOTweenのアニメーションを確実に動かす
                .WithCancellation(this.GetCancellationTokenOnDestroy());
        }

        // 5. タイトルシーンをロード
        await SceneManager.LoadSceneAsync("TitleScene").ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }
    private void OnDestroy()
    {
        // オブジェクト破棄時にイベント購読を解除（エラー防止）
        if (pauseController != null)
        {
            pauseController.OnPauseToggled -= HandlePauseUI;
        }
    }

    private void HandlePauseUI(bool isPaused)
    {
        if (isPaused)
        {
            pausePanel.SetActive(true);

            // 開ききったら一番上のボタンを自動選択
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
        else
        {
            // --- ポーズ画面を閉じる演出 ---
            pausePanel.SetActive(false);
        }
    }
}
