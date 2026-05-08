using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MusicTimer : MonoBehaviour
{
    [Header("通知設定")]
    [SerializeField] private float notifyTime = 96f; // 進行イベントを呼ぶ時間

    // イベント定義
    public static event Action OnGameProgressed;
    public static event Action OnGameEnded;

    // タイマーを途中でキャンセルするためのトークン
    private CancellationTokenSource _cts;

    void OnEnable()
    {
        AudioManager.Instance.OnBGMStarted += StartTimer;
    }

    void OnDisable()
    {
        AudioManager.Instance.OnBGMStarted -= StartTimer;
        CancelTimer(); // スクリプトが無効になったらタイマーも止める
    }

    private void StartTimer(AudioClip clip, AudioSource source)
    {
        // 判定（※名前での判定はタイポの危険があるので注意）
        if (clip.name == "Mountain_King")
        {
            CancelTimer(); // 前のタイマーが動いていたらリセット
            _cts = new CancellationTokenSource();

            // 非同期でタイマーをスタート（完了を待たずに次の処理へ）
            RunTimerAsync(clip.length, _cts.Token).Forget();
        }
    }

    private async UniTaskVoid RunTimerAsync(float totalLength, CancellationToken token)
    {
        try
        {
            // 1. まず「notifyTime（96秒）」だけ待つ
            await UniTask.Delay(TimeSpan.FromSeconds(notifyTime), cancellationToken: token);

            // 時間が来たらイベント発行
            OnGameProgressed?.Invoke();

            // 2. 残りの時間（曲の全長 - 96秒）を計算して待つ
            float remainingTime = totalLength - notifyTime - 1.0f;
            if (remainingTime > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainingTime), cancellationToken: token);
            }

            // 曲が完全に終わったらイベント発行
            OnGameEnded?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // CancelTimer() が呼ばれた時は、エラーを出さずにここで安全に処理を終了します
        }
    }

    // タイマーを安全に破棄する処理
    private void CancelTimer()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}