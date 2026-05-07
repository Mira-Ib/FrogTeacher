using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeItemView : MonoBehaviour
{
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Image[] _circleImages;
    [SerializeField] private Sprite _filledCircleSprite;
    [SerializeField] private Sprite _emptyCircleSprite;
    [SerializeField] private Button _testPlayButton; // 追加：右隣のスピーカーアイコンボタン

    public event Action<int> OnVolumeChanged;
    public event Action OnTestPlayClicked; // 追加：テスト再生要求イベント

    private void Awake()
    {
        _volumeSlider.onValueChanged.AddListener(value =>
        {
            int intValue = Mathf.RoundToInt(value);
            UpdateVisuals(intValue);
            OnVolumeChanged?.Invoke(intValue);
        });

        // テストボタンが押されたらイベントを発火
        _testPlayButton.onClick.AddListener(() => OnTestPlayClicked?.Invoke());
    }

    public void Initialize(int currentVolume)
    {
        _volumeSlider.value = currentVolume;

        // ★追加：スライダーのイベント発火に頼らず、直接見た目を更新する！
        UpdateVisuals(currentVolume);
    }

    /// <summary>
    /// 7つのImage（○）の見た目を更新
    /// </summary>
    private void UpdateVisuals(int volume)
    {
        for (int i = 0; i < _circleImages.Length; i++)
        {
            _circleImages[i].sprite = (i < volume) ? _filledCircleSprite : _emptyCircleSprite;
        }
    }

    // 初期フォーカス用
    public GameObject GetSliderGameObject() => _volumeSlider.gameObject;

    // VolumeItemView.cs 内に追加
    public void ClearAllEvents()
    {
        OnVolumeChanged = null;
        OnTestPlayClicked = null;
    }
}