using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class OptionScreenManager : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private VolumeItemView _bgmView;
    [SerializeField] private VolumeItemView _seView;
    [SerializeField] private Button _submitButton;

    [Header("Transition References")]
    [SerializeField] private NextScreenPanel _optionsContentPanel;
    [SerializeField] private RectTransform _submitButtonRect;
    [SerializeField] private NextScreenPanel _titleScreenPanel;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _bgmTestClip;
    [SerializeField] private AudioClip _seTestClip;

    [Header("Transition Effects")]
    [SerializeField] private MaskTransitionEffect _maskTransitionEffect;
    [SerializeField] private TitleTransitionCoordinator _titleCoordinator; // ★インスペクタから追加できるようにする

    // Controllerの保持
    private OptionScreenController _controller;

    private void Start()
    {
        // ※SettingsManagerとAudioManagerは、プロジェクトの構成に合わせて取得してください
        // （例：ServiceLocatorパターンや、FindObjectOfTypeなど）
        var settingsManager = new SettingsManager(new JsonSettingsRepository("settings.json"));
        var audioManager = FindFirstObjectByType<AudioManager>();

        // Controllerを生成し、インスペクタで設定した参照を全て渡す
        _controller = new OptionScreenController(
            _bgmView,
            _seView,
            _submitButton,
            settingsManager,
            audioManager,
            _bgmTestClip,
            _seTestClip,
            _optionsContentPanel,
            _submitButtonRect,
            _maskTransitionEffect,
            _titleCoordinator, // ★ここに追加
            this.GetCancellationTokenOnDestroy()
        );
        // ★追加: 初回起動時のフォーカス設定
        _controller.SetInitialFocus();
    }

    // ★追加：オブジェクトがアクティブ（画面が降ってきた）になる度に呼ばれるUnity標準機能
    private void OnEnable()
    {
        // _controllerが生成済みならフォーカスを当てる
        // (タイトル画面から何度も行き来するたびにBGMスライダーにフォーカスが当たります)
        if (_controller != null)
        {
            _controller.SetInitialFocus();
        }
    }

    private void OnDestroy()
    {
        // Managerが破棄される時に、ControllerのDisposeを呼んでメモリを解放
        _controller?.Dispose();
    }
}