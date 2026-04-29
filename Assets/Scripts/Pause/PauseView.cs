using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseView : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private PauseController pauseController;

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton; // 「ゲームに戻る」ボタン（一番上）
    [SerializeField] private AudioSource pauseAudioSource;

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
