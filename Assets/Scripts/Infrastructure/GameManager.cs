using UnityEngine;
using System;

public enum GameState
{
    Title,
    Lecture,
    Quiz,
    Result
}

public class GameManager : MonoBehaviour
{
    // シングルトンの実装
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    // 状態が変更された時に通知するイベント
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初期状態をタイトルに設定
        ChangeState(GameState.Title);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // 各マネージャーに状態変化を通知（疎結合）
        OnStateChanged?.Invoke(newState);

        Debug.Log($"Game State Changed to: {newState}");

        // 状態ごとの初期化処理の呼び出し例
        HandleStateEntry(newState);
    }

    private void HandleStateEntry(GameState state)
    {
        switch (state)
        {
            case GameState.Title:
                // タイトル画面の初期化、BGM再生要求など
                break;
            case GameState.Lecture:
                // カエル先生の講義準備
                break;
            case GameState.Quiz:
                // クイズ生成
                break;
            case GameState.Result:
                // スコア計算、セーブ保存
                break;
        }
    }
}