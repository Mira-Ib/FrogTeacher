using System;
using System.Collections.Generic;
using UnityEngine;

// 1. 曲と効果音の名前をリストアップする（Enum）
public enum BGM { Title, Lecture, Quiz }
public enum SE { Click, Transition, Chalk, Correct_1, Correct_2, Correct_3, Correct_4, Correct_5, Correct_6, Correct_7, Correct_8, Wrong, }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public event Action<AudioClip, AudioSource> OnBGMStarted;

    [Header("スピーカー (Audio Sources)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource pauseseSource;

    // 2. インスペクターで設定するための専用クラス
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
    // 再生機能 (Enumを受け取って再生する)
    // ==========================================
    public void PlayBGM(BGM bgmType, bool loop = true)
    {
        // リストの中から、指定されたBGMと一致するデータを検索
        BGMDict data = bgmList.Find(x => x.bgmType == bgmType);
        if (data == null || data.clip == null) return;

        if (bgmSource.clip == data.clip) return;

        bgmSource.loop = loop;
        bgmSource.clip = data.clip;
        bgmSource.Play();

        OnBGMStarted?.Invoke(data.clip, bgmSource);
    }

    public void PlaySE(SE seType)
    {
        SEDict data = seList.Find(x => x.seType == seType);
        if (data != null && data.clip != null)
        {
            seSource.PlayOneShot(data.clip);
        }
    }

    // ポーズ専用のSE再生
    public void PlayPauseSE(SE seType)
    {
        SEDict data = seList.Find(x => x.seType == seType);
        if (data != null && data.clip != null)
        {
            pauseseSource.PlayOneShot(data.clip);
        }
    }
}