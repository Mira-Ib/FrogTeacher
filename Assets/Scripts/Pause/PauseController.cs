using System;
using UnityEngine;
using UnityEngine.EventSystems; // ★追加：UIフォーカスのために必要
using Cysharp.Threading.Tasks;  // ★追加：1フレーム待機（UniTask）のために必要

public class PauseController : MonoBehaviour
{
    // Viewにポーズ状態の変更を知らせるイベント
    public event Action<bool> OnPauseToggled;

    // ==========================================
    // ★追加：フォーカス設定
    // ==========================================
    [Header("UI Focus")]
    [Tooltip("ポーズ解除時にフォーカスを戻したいゲーム画面のボタン（回答ボタンなど）")]
    [SerializeField] private GameObject firstFocusButtonOnResume;

    private bool _isPaused = false;

    private void Update()
    {
        // Pキーでポーズ切り替え
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // UIのポーズアイコンがクリックされた時にも呼べるように public にしておきます
    public void TogglePause()
    {
        _isPaused = !_isPaused;

        // 三項演算子で、trueなら0(停止)、falseなら1(通常)を代入
        Time.timeScale = _isPaused ? 0f : 1f;

        // ゲーム内のすべての音を一時停止／再開する
        AudioListener.pause = _isPaused;

        // Viewへ現在の状態を通知
        OnPauseToggled?.Invoke(_isPaused);

        // ==========================================
        // ★追加：ポーズを解除（再開）した時にフォーカスを戻す
        // ==========================================
        if (!_isPaused)
        {
            RestoreFocusAsync().Forget();
        }
    }

    // 「ゲームに戻る」ボタンなどから明示的に解除したい場合用
    public void ResumeGame()
    {
        if (_isPaused)
        {
            TogglePause();
        }
    }

    // ==========================================
    // ★追加：1フレーム待ってからフォーカスを当てる処理
    // ==========================================
    private async UniTaskVoid RestoreFocusAsync()
    {
        // View側のUI（ポーズ画面）が非表示になるのを1フレームだけ待つ
        // （ポーズ画面が消えきる前にフォーカスを当てようとすると失敗するため）
        await UniTask.Yield(this.GetCancellationTokenOnDestroy());

        if (EventSystem.current != null && firstFocusButtonOnResume != null)
        {
            // 一度nullを入れて選択状態を完全にリセットしてから、指定のボタンを選択する
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstFocusButtonOnResume);
        }
    }
}