using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTweenを追加

public enum BGM { Title, Lecture, Quiz }
// SEに「Talking」を追加しました
public enum SE { Click, Talking, Chalk, Chime, Correct_1, Correct_2, Correct_3, Correct_4, Correct_5, Correct_6, Correct_7, Correct_8, Wrong }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public event Action<AudioClip, AudioSource> OnBGMStarted;

    [Header("スピーカー (Audio Sources)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource pauseseSource;
    // 文字送りの「ポポポ…」という音など、途中で止めたい音専用のスピーカーを追加
    [SerializeField] private AudioSource loopSeSource;

    [System.Serializable]
    public class BGMDict
    {
        public BGM bgmType;
        public AudioClip clip;
    }

    [System.Serializable]
    public class SEDict
    {
        public SE seType;
        public AudioClip clip;
    }

    [Header("オーディオデータ")]
    [SerializeField] private List<BGMDict> bgmList = new List<BGMDict>();
    [SerializeField] private List<SEDict> seList = new List<SEDict>();

    private Tween _bgmFadeTween; // BGMフェード用のTweenを保持

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

    // ==========================================
    // BGM 再生・フェード機能
    // ==========================================
    public void PlayBGM(BGM bgmType, bool loop = true)
    {
        BGMDict data = bgmList.Find(x => x.bgmType == bgmType);
        if (data == null || data.clip == null) return;
        if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

        // フェード処理が動いていればキャンセルして音量を戻す
        _bgmFadeTween?.Kill();
        bgmSource.volume = 1f;

        bgmSource.loop = loop;
        bgmSource.clip = data.clip;
        bgmSource.Play();

        OnBGMStarted?.Invoke(data.clip, bgmSource);
    }

    public void FadeOutBGM(float duration)
    {
        // DOTweenを使って指定秒数で音量を0にし、完了したらStopする
        _bgmFadeTween = bgmSource.DOFade(0f, duration).OnComplete(() => bgmSource.Stop());
    }

    public void StopBGMImmediate()
    {
        _bgmFadeTween?.Kill();
        bgmSource.volume = 0f;
        bgmSource.Stop();
    }

    // ==========================================
    // SE 再生機能
    // ==========================================
    public void PlaySE(SE seType)
    {
        SEDict data = seList.Find(x => x.seType == seType);
        if (data != null && data.clip != null)
        {
            seSource.PlayOneShot(data.clip);
        }
    }

    public void PlayPauseSE(SE seType)
    {
        SEDict data = seList.Find(x => x.seType == seType);
        if (data != null && data.clip != null)
        {
            pauseseSource.PlayOneShot(data.clip);
        }
    }

    // ==========================================
    // ループSE（文字送り等）専用の機能
    // ==========================================
    public void PlayLoopSE(SE seType)
    {
        SEDict data = seList.Find(x => x.seType == seType);
        if (data != null && data.clip != null)
        {
            loopSeSource.clip = data.clip;
            loopSeSource.loop = true;  // ループをオンにして再生開始
            loopSeSource.Play();
        }
    }

    /// <summary>
    /// ループSEを停止する
    /// </summary>
    /// <param name="forceStop">trueなら即座に停止、falseなら現在の1ループが鳴り終わってから自然停止</param>
    public void StopLoopSE(bool forceStop = false)
    {
        if (forceStop)
        {
            // スキップ時など：即座に音をぶつ切りにして止める
            loopSeSource.Stop();
        }
        else
        {
            // 通常時：ループ設定だけを解除する。
            // これにより、今再生されているフレーズの最後まで鳴り切ってから美しく停止します。
            loopSeSource.loop = false;
        }
    }
}