using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

/// <summary>
/// オプション画面のUI、データ保存、音声再生、そして画面遷移演出を統括するコントローラー
/// （MonoBehaviourを継承しない純粋なC#クラス）
/// </summary>
public class OptionScreenController : IDisposable
{
    // --- Viewの参照 ---
    private readonly VolumeItemView _bgmView;
    private readonly VolumeItemView _seView;
    private readonly Button _submitButton;

    // --- Manager / Data / Audioの参照 ---
    private readonly SettingsManager _settingsManager;
    private readonly AudioManager _audioManager;
    private readonly AudioClip _bgmTestClip;
    private readonly AudioClip _seTestClip;

    // --- 画面遷移演出用の参照 ---
    private readonly NextScreenPanel _optionsContentPanel;
    private readonly RectTransform _submitButtonRect;
    private readonly MaskTransitionEffect _maskTransitionEffect;
    private readonly TitleTransitionCoordinator _titleCoordinator;
    private readonly CancellationToken _token;
    private readonly float _submitButtonDefaultPosY;

    /// <summary>
    /// コンストラクタ：必要な参照を全て外部から注入する (Dependency Injection)
    /// </summary>
    public OptionScreenController(
        VolumeItemView bgmView,
        VolumeItemView seView,
        Button submitButton,
        SettingsManager settingsManager,
        AudioManager audioManager,
        AudioClip bgmTestClip,
        AudioClip seTestClip,
        NextScreenPanel optionsContentPanel,
        RectTransform submitButtonRect,
        MaskTransitionEffect maskTransitionEffect,
        TitleTransitionCoordinator titleCoordinator,
        CancellationToken token)
    {
        _bgmView = bgmView;
        _seView = seView;
        _submitButton = submitButton;
        _settingsManager = settingsManager;
        _audioManager = audioManager;
        _bgmTestClip = bgmTestClip;
        _seTestClip = seTestClip;
        _optionsContentPanel = optionsContentPanel;
        _submitButtonRect = submitButtonRect;
        _maskTransitionEffect = maskTransitionEffect;
        _titleCoordinator = titleCoordinator;
        _token = token;

        // 起動時の正しいY座標を記憶する
        _submitButtonDefaultPosY = _submitButtonRect.anchoredPosition.y;

        // 初期化処理の実行
        BindEvents();
        InitializeViews();
    }

    /// <summary>
    /// 各種UIのイベントを登録する
    /// </summary>
    private void BindEvents()
    {
        // --- BGMのイベント ---
        _bgmView.OnVolumeChanged += volume =>
        {
            _settingsManager.UpdateBgmVolume(volume);
            _audioManager.SetBgmVolume(volume);
        };
        _bgmView.OnTestPlayClicked += () => _audioManager.PlayBgmTestAudioAsync(_bgmTestClip).Forget();

        // --- SEのイベント ---
        _seView.OnVolumeChanged += volume =>
        {
            _settingsManager.UpdateSeVolume(volume);
            _audioManager.SetSeVolume(volume);
        };
        _seView.OnTestPlayClicked += () => _audioManager.PlaySeTestAudio(_seTestClip);

        // --- 決定ボタンのイベント ---
        _submitButton.onClick.AddListener(() => OnSubmitClickedAsync().Forget());
    }

    /// <summary>
    /// 現在のセーブデータを読み込んでViewに反映させる
    /// </summary>
    private void InitializeViews()
    {
        _bgmView.Initialize(_settingsManager.CurrentData.BgmVolume);
        _seView.Initialize(_settingsManager.CurrentData.SeVolume);
    }

    /// <summary>
    /// キーボード・コントローラー操作の起点として、BGMスライダーにフォーカスを当てる
    /// </summary>
    public void SetInitialFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(_bgmView.GetSliderGameObject());
        }
    }

    /// <summary>
    /// 決定ボタンが押された時の画面遷移処理（非同期）
    /// </summary>
    private async UniTaskVoid OnSubmitClickedAsync()
    {
        _submitButton.interactable = false;
        _maskTransitionEffect.ProtectObjects(_submitButtonRect.gameObject);

        // 決定ボタンを落下させるアニメーション（UniTask化）
        UniTask buttonDropTask = _submitButtonRect.DOAnchorPosY(_submitButtonDefaultPosY - 1000f, 0.4f)
            .SetEase(Ease.InQuad)
            .WithCancellation(_token);

        // コーディネーターにお任せ！（ボタン落下のタスクを一緒に渡す）
        await _titleCoordinator.ReturnToTitleAsync(_optionsContentPanel, buttonDropTask);

        // お掃除
        _maskTransitionEffect.ResetMaskAndParents();
        _submitButtonRect.anchoredPosition = new Vector2(_submitButtonRect.anchoredPosition.x, _submitButtonDefaultPosY);
        _submitButton.interactable = true;
    }

    /// <summary>
    /// クラスが破棄される際に呼ばれる、メモリリーク防止のためのイベント解除処理
    /// </summary>
    public void Dispose()
    {
        if (_bgmView != null)
        {
            _bgmView.ClearAllEvents();
        }

        if (_seView != null)
        {
            _seView.ClearAllEvents();
        }

        if (_submitButton != null)
        {
            _submitButton.onClick.RemoveAllListeners();
        }
    }
}