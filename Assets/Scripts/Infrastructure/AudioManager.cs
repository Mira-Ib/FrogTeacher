using Cysharp.Threading.Tasks;
using DG.Tweening; // DOTweenを追加
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum BGM { Title, Lecture, Quiz }
// SEに「Talking」を追加しました
public enum SE { Click, Transition, Chalk, Chime, Correct_1, Correct_2, Correct_3, Correct_4, Correct_5, Correct_6, Correct_7, Correct_8, Wrong, Talking, Back }

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

    [Header("Audio Mixer (使用している場合のみアタッチ)")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _bgmExposedParam = "BgmVolume";
    [SerializeField] private string _seExposedParam = "SeVolume";

    [Header("Volume Mapping (0〜7段階のdB設定)")]
    [Tooltip("インデックス0がミュート、7が最大音量。後からインスペクタで微調整できます。")]
    [SerializeField]
    private float[] _volumeDbSteps = new float[8]
    {
        -80f, // 0: ミュート (AudioMixerの最低値)
        -30f, // 1: かなり小さめ
        -20f, // 2
        -14f, // 3
        -9f,  // 4
        -5f,  // 5
        -2f,  // 6
         0f   // 7: 最大音量 (原音そのまま)
    };

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
    // 音量設定メソッド (Controllerから呼ばれる)
    // ==========================================

    /// <summary>
    /// BGMの音量を0〜7の段階で設定する
    /// </summary>
    public void SetBgmVolume(int level)
    {
        // 念のため配列の範囲外アクセスを防ぐ
        level = Mathf.Clamp(level, 0, _volumeDbSteps.Length - 1);
        float dbVolume = _volumeDbSteps[level];

        if (_audioMixer != null)
        {
            // AudioMixerが設定されている場合は、Exposed Parameter経由でdBを直接流し込む
            _audioMixer.SetFloat(_bgmExposedParam, dbVolume);
        }
    }

    /// <summary>
    /// SEの音量を0〜7の段階で設定する
    /// </summary>
    public void SetSeVolume(int level)
    {
        level = Mathf.Clamp(level, 0, _volumeDbSteps.Length - 1);
        float dbVolume = _volumeDbSteps[level];

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat(_seExposedParam, dbVolume);
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
    /// <summary>
    /// BGMのテスト音声を再生し、終了後に元のBGMを再開する
    /// </summary>
    // ★追加：テスト再生中かを判定するフラグ
    private bool _isPlayingBgmTest = false;
    public async UniTask PlayBgmTestAudioAsync(AudioClip testClip)
    {
        // ★追加：すでにテスト再生中なら、何もしない（連打防止）
        if (_isPlayingBgmTest) return;

        _isPlayingBgmTest = true; // フラグを立てる

        try
        {
            AudioClip currentClip = bgmSource.clip;
            float currentTime = bgmSource.time;
            bool wasPlaying = bgmSource.isPlaying;

            bgmSource.clip = testClip;
            bgmSource.time = 0f;
            bgmSource.Play();

            await UniTask.Delay(System.TimeSpan.FromSeconds(testClip.length));

            bgmSource.clip = currentClip;
            if (wasPlaying)
            {
                bgmSource.time = currentTime;
                bgmSource.Play();
            }
        }
        finally
        {
            // ★追加：エラーが起きても途中でキャンセルされても、必ずフラグを下ろす
            _isPlayingBgmTest = false;
        }
    }

    // AudioManager.cs に追記
    public void PlaySeTestAudio(AudioClip testClip)
    {
        // 現在のSEの音量設定（_seSource.volume）に従って、一回だけ再生する
        seSource.PlayOneShot(testClip);
    }
}