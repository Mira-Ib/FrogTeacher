using System;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    // Viewにポーズ状態の変更を知らせるイベント
    public event Action<bool> OnPauseToggled;

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

        // 【追加】ゲーム内のすべての音を一時停止／再開する
        AudioListener.pause = _isPaused;

        // Viewへ現在の状態を通知
        OnPauseToggled?.Invoke(_isPaused);
    }

    // 「ゲームに戻る」ボタンなどから明示的に解除したい場合用
    public void ResumeGame()
    {
        if (_isPaused)
        {
            TogglePause();
        }
    }
}