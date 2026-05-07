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
    private readonly TitleTransitionCoordinator _titleCoordinator; // ★追加：タイトル画面のリセット用
    private readonly CancellationToken _token;
    private readonly float _submitButtonDefaultPosY; // ★復活：決定ボタンの元の高さを記憶する

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
        TitleTransitionCoordinator titleCoordinator, // ★追加
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
        _titleCoordinator = titleCoordinator; // ★追加
        _token = token;

        // ★追加：起動時の正しいY座標を記憶する
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
            _settingsManager.UpdateBgmVolume(volume); // データ保存
            _audioManager.SetBgmVolume(volume);       // 実際の音量に反映
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
        // 非同期メソッドを呼ぶため、ラムダ式でラップしてForget()をつける
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
        // EventSystemが存在し、かつBGMスライダーのGameObjectが存在するか確認
        if (EventSystem.current != null)
        {
            // EventSystem に「最初からBGMスライダーを選択状態にして」と命令する
            EventSystem.current.SetSelectedGameObject(_bgmView.GetSliderGameObject());
        }
    }

    /// <summary>
    /// 決定ボタンが押された時の画面遷移処理（非同期）
    /// </summary>
    private async UniTaskVoid OnSubmitClickedAsync()
    {
        // 1. ボタン連打防止
        _submitButton.interactable = false;

        // 2. タイトルのUIをSafeContainerから回収し、タイトル画面を上空にセットする
        _titleCoordinator.PrepareForReturn();

        // 3. 今度は決定ボタンをSafeContainerへ避難させる
        _maskTransitionEffect.ProtectObjects(_submitButtonRect.gameObject);

        // ==========================================
        // ★修正：アニメーションの順番を変更
        // ==========================================

        // ① まず、オプション画面の中身をマスクで消去し、完全に終わるまで待機する
        await _optionsContentPanel.WipeOutAsync(_token);

        // ② その後、決定ボタンの落下 と タイトル画面の降下 を同時に行う
        await UniTask.WhenAll(
            _submitButtonRect.DOAnchorPosY(_submitButtonRect.anchoredPosition.y - 1000f, 0.4f).SetEase(Ease.InQuad).WithCancellation(_token),
            _titleCoordinator.ExecuteDropInAsync(_token)
        );

        // ==========================================

        // 4. 黒板のアニメーションを再開
        _titleCoordinator.StartBackgroundAnimation();

        // ★追加：次回開く時のために、決定ボタンを元の階層・座標・状態にリセットする
        _maskTransitionEffect.ResetMaskAndParents(); // 決定ボタンをオプションパネル内に戻す
        _submitButtonRect.anchoredPosition = new Vector2(_submitButtonRect.anchoredPosition.x, _submitButtonDefaultPosY); // 座標を元に戻す
        _submitButton.interactable = true; // ボタンを再び押せるようにする

        Debug.Log("タイトル画面への遷移演出が完了しました");
    }

    /// <summary>
    /// クラスが破棄される際に呼ばれる、メモリリーク防止のためのイベント解除処理
    /// </summary>
    public void Dispose()
    {
        if (_bgmView != null)
        {
            _bgmView.ClearAllEvents(); // VolumeItemView側に追加したクリアメソッド
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