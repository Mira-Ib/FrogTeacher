using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening; // ★追加：DOFade（暗転アニメーション）を使うために必要です

public class StageSelectController : IDisposable
{
    // --- UIの参照 ---
    private readonly TextMeshProUGUI _stageNameText;
    private readonly TextMeshProUGUI _descriptionText;
    private readonly Button _leftButton;
    private readonly Button _rightButton;
    private readonly Button _submitButton; // ステージ名ボタン（決定兼用）
    private readonly Button _backButton;

    // --- データと遷移の参照 ---
    private readonly List<StageData> _stageDataList;
    private readonly TitleTransitionCoordinator _titleCoordinator;
    private readonly NextScreenPanel _myPanel;
    private readonly CanvasGroup _blackoutCanvasGroup; // ★追加：暗転用のキャンバスグループ
    private readonly CancellationToken _token;

    // --- 状態管理 ---
    private int _currentIndex = 0;

    public StageSelectController(
        TextMeshProUGUI stageNameText,
        TextMeshProUGUI descriptionText,
        Button leftButton,
        Button rightButton,
        Button submitButton,
        Button backButton,
        List<StageData> stageDataList,
        TitleTransitionCoordinator titleCoordinator,
        NextScreenPanel myPanel,
        CanvasGroup blackoutCanvasGroup, // ★追加：Managerから受け取る
        CancellationToken token)
    {
        _stageNameText = stageNameText;
        _descriptionText = descriptionText;
        _leftButton = leftButton;
        _rightButton = rightButton;
        _submitButton = submitButton;
        _backButton = backButton;
        _stageDataList = stageDataList;
        _titleCoordinator = titleCoordinator;
        _myPanel = myPanel;
        _blackoutCanvasGroup = blackoutCanvasGroup; // ★追加：変数に保存
        _token = token;

        BindEvents();
        UpdateUI(); // 最初は0番目のデータを表示
    }

    private void BindEvents()
    {
        // ◀ 左ボタン：インデックスを減らす（0より小さくなったら最後尾にループ）
        _leftButton.onClick.AddListener(() =>
        {
            if (_stageDataList.Count == 0) return;
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _stageDataList.Count - 1;
            UpdateUI();
        });

        // ▶ 右ボタン：インデックスを増やす（最大数を超えたら0にループ）
        _rightButton.onClick.AddListener(() =>
        {
            if (_stageDataList.Count == 0) return;
            _currentIndex++;
            if (_currentIndex >= _stageDataList.Count) _currentIndex = 0;
            UpdateUI();
        });

        // 決定ボタン（ステージ名）：GameSceneへ進む
        _submitButton.onClick.AddListener(() => OnSubmitClickedAsync().Forget());

        // 戻るボタン：タイトルへ戻る
        _backButton.onClick.AddListener(() => OnBackButtonClickedAsync().Forget());
    }

    private void UpdateUI()
    {
        if (_stageDataList == null || _stageDataList.Count == 0)
        {
            _stageNameText.text = "データなし";
            _descriptionText.text = "ステージデータが登録されていません。";
            return;
        }

        StageData currentData = _stageDataList[_currentIndex];
        _stageNameText.text = currentData.stageName;
        _descriptionText.text = currentData.description;
    }

    private async UniTaskVoid OnBackButtonClickedAsync()
    {
        _backButton.interactable = false;

        // 戻るボタンの避難（Protect）は不要。そのままコーディネーターにお任せ！
        await _titleCoordinator.ReturnToTitleAsync(_myPanel);

        // ※UIのリセット（interactableの復元など）は NextScreenPanel の OnEnable が自動でやってくれます
    }

    private async UniTaskVoid OnSubmitClickedAsync()
    {
        _submitButton.interactable = false;
        _leftButton.interactable = false;
        _rightButton.interactable = false;
        _backButton.interactable = false;

        // 1. 選択したステージデータを共有スペース（GameSessionData）に保存する！
        if (_stageDataList.Count > 0)
        {
            GameSessionData.SelectedStage = _stageDataList[_currentIndex];
        }

        // 2. チャイムを鳴らし、BGMをフェードアウトする
        AudioManager.Instance.PlaySE(SE.Chime);
        AudioManager.Instance.FadeOutBGM(0.5f);

        // 3. 画面を暗転させる（フェードアウト）
        if (_blackoutCanvasGroup != null)
        {
            _blackoutCanvasGroup.gameObject.SetActive(true);
            await _blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(_token);
        }
        else
        {
            // パネルが無い場合は時間経過だけ待つ
            await UniTask.Delay(1000, cancellationToken: _token);
        }

        // 4. GameSceneをロードする
        // .ToUniTask() をつけることで、ロード完了を待機（await）できるようになります
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene")
            .ToUniTask(cancellationToken: _token);
    }

    public void Dispose()
    {
        _leftButton.onClick.RemoveAllListeners();
        _rightButton.onClick.RemoveAllListeners();
        _submitButton.onClick.RemoveAllListeners();
        _backButton.onClick.RemoveAllListeners();
    }
}