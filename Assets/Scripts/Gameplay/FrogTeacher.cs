using System;
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを扱うために必要

// Imageコンポーネントが必須であることを保証
[RequireComponent(typeof(Image))]
public class FrogTeacher : MonoBehaviour
{
    [Serializable]
    public struct SpritePair
    {
        public Sprite frame1;
        public Sprite frame2;
    }

    [Header("アニメーション設定")]
    [Tooltip("画像が切り替わる秒数")]
    [SerializeField] private float swapInterval = 0.5f;

    [Tooltip("進行度ごとの画像ペア（上から順に進行）")]
    [SerializeField] private SpritePair[] spritePairs;

    private Image _image;
    private int _currentProgressIndex = 0;
    private float _timer = 0f;
    private bool _isShowingFirstFrame = true;

    private void Awake()
    {
        // Imageコンポーネントを取得
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateSprite();
        MusicTimer.OnGameProgressed += HandleGameProgressed;
    }

    private void OnDestroy()
    {
        MusicTimer.OnGameProgressed -= HandleGameProgressed;
    }

    private void Update()
    {
        if (spritePairs == null || spritePairs.Length == 0) return;

        _timer += Time.deltaTime;

        if (_timer >= swapInterval)
        {
            _timer -= swapInterval;
            _isShowingFirstFrame = !_isShowingFirstFrame;
            UpdateSprite();
        }
    }

    /// <summary>
    /// ゲーム進行イベントに合わせて画像を次のペアに切り替える
    /// </summary>
    public void HandleGameProgressed()
    {
        if (spritePairs == null) return;

        _currentProgressIndex++;

        // 用意したペアの数を超えないように制限
        if (_currentProgressIndex >= spritePairs.Length)
        {
            _currentProgressIndex = spritePairs.Length - 1;
        }

        // 進行した瞬間にタイマーとフレームをリセットして即座に反映
        _timer = 0f;
        _isShowingFirstFrame = true;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (spritePairs == null || spritePairs.Length == 0) return;

        SpritePair currentPair = spritePairs[_currentProgressIndex];

        // Imageのspriteを差し替え
        _image.sprite = _isShowingFirstFrame ? currentPair.frame1 : currentPair.frame2;
    }
}
