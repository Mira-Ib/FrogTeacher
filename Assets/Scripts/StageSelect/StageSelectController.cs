using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMP_Ruby;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class StageSelectController : IDisposable
{
    // --- UIの参照（★TextMeshProUGUI から TextMeshProRuby に変更） ---
    private readonly TextMeshProRuby _stageNameText;
    private readonly TextMeshProRuby _descriptionText;
    private readonly Button _leftButton;
    private readonly Button _rightButton;
    private readonly Button _submitButton;
    private readonly Button _backButton;

    // --- データと遷移の参照 ---
    private readonly List<StageData> _stageDataList;
    private readonly TitleTransitionCoordinator _titleCoordinator;
    private readonly NextScreenPanel _myPanel;
    private readonly CanvasGroup _blackoutCanvasGroup;
    private readonly CancellationToken _token;

    // --- 状態管理 ---
    private int _currentIndex = 0;

    public StageSelectController(
        TextMeshProRuby stageNameText,       // ★引数の型を変更
        TextMeshProRuby descriptionText,     // ★引数の型を変更
        Button leftButton,
        Button rightButton,
        Button submitButton,
        Button backButton,
        List<StageData> stageDataList,
        TitleTransitionCoordinator titleCoordinator,
        NextScreenPanel myPanel,
        CanvasGroup blackoutCanvasGroup,
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
        _blackoutCanvasGroup = blackoutCanvasGroup;
        _token = token;

        BindEvents();
        UpdateUI();
    }

    private void BindEvents()
    {
        _leftButton.onClick.AddListener(() =>
        {
            if (_stageDataList.Count == 0) return;
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _stageDataList.Count - 1;
            UpdateUI();
        });

        _rightButton.onClick.AddListener(() =>
        {
            if (_stageDataList.Count == 0) return;
            _currentIndex++;
            if (_currentIndex >= _stageDataList.Count) _currentIndex = 0;
            UpdateUI();
        });

        _submitButton.onClick.AddListener(() => OnSubmitClickedAsync().Forget());
        _backButton.onClick.AddListener(() => OnBackButtonClickedAsync().Forget());
    }

    // ==================================================
    // ★UpdateUI の修正
    // ==================================================
    private void UpdateUI()
    {
        if (_stageDataList == null || _stageDataList.Count == 0)
        {
            // TextMeshProRuby も .text プロパティ（または .Text）で代入可能です
            _stageNameText.Text = "データなし";
            _descriptionText.Text = "ステージデータが登録されていません。";
            return;
        }

        StageData currentData = _stageDataList[_currentIndex];

        // Rubyタグ（<r=ルビ>漢字</r>など）が含まれていても、そのまま代入すれば反映されます
        _stageNameText.Text = currentData.stageName;
        _descriptionText.Text = currentData.description;
    }

    private async UniTaskVoid OnBackButtonClickedAsync()
    {
        _backButton.interactable = false;
        await _titleCoordinator.ReturnToTitleAsync(_myPanel);
    }

    private async UniTaskVoid OnSubmitClickedAsync()
    {
        _submitButton.interactable = false;
        _leftButton.interactable = false;
        _rightButton.interactable = false;
        _backButton.interactable = false;

        if (_stageDataList.Count > 0)
        {
            GameSessionData.SelectedStage = _stageDataList[_currentIndex];
        }

        AudioManager.Instance.PlaySE(SE.Chime);
        AudioManager.Instance.FadeOutBGM(0.5f);

        if (_blackoutCanvasGroup != null)
        {
            _blackoutCanvasGroup.gameObject.SetActive(true);
            await _blackoutCanvasGroup.DOFade(1f, 1.0f).WithCancellation(_token);
        }
        else
        {
            await UniTask.Delay(1000, cancellationToken: _token);
        }

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