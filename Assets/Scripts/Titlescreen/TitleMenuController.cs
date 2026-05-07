using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleMenuController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private TitleTransitionCoordinator coordinator;

    [Header("落下演出の設定")]
    [SerializeField] private float dropDistance = 1500f;
    [SerializeField] private float dropDuration = 0.6f;
    [SerializeField] private Ease dropEase = Ease.InQuad;

    // ★追加：タイトルに戻った時にフォーカスを当てるUI
    [Header("UIナビゲーション")]
    [Tooltip("タイトル画面で最初に選択状態にするボタン")]
    [SerializeField] private GameObject defaultSelectedButton;

    // タイトルに戻ってきた時に位置を戻すため、初期Y座標を記憶しておく辞書
    private Dictionary<RectTransform, float> originalYPositions = new Dictionary<RectTransform, float>();

    private void Awake()
    {
        // 起動時に、子要素にある全ボタンの初期位置を記憶する
        var buttons = GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            var rect = btn.GetComponent<RectTransform>();
            originalYPositions.Add(rect, rect.anchoredPosition.y);
        }
    }

    /// <summary>
    /// ★インスペクタの OnClick() に登録するメソッド（引数は GameObject 1つだけ！）
    /// </summary>
    public void OnMenuSubmit(GameObject selectedMenuObject)
    {
        // 1. ボタン自身にアタッチされた「目的地データ」を取得する
        var btnData = selectedMenuObject.GetComponent<MenuButtonData>();
        if (btnData == null || btnData.targetPanel == null)
        {
            Debug.LogError("MenuButtonData がアタッチされていないか、Target Panel が設定されていません！", selectedMenuObject);
            return;
        }

        // 2. 多重押しを防止
        DisableAllButtons();

        // 3. スクリプト内から、引数2つのメソッドを呼び出して処理を委譲する
        coordinator.OnMenuSelected(selectedMenuObject, btnData.targetPanel);
    }

    // 選択肢の落下演出
    public async UniTask DropSelectedMenuAsync(GameObject selectedMenu, CancellationToken token)
    {
        if (selectedMenu == null) return;
        var targetRect = selectedMenu.GetComponent<RectTransform>();
        if (targetRect == null) return;

        await targetRect.DOAnchorPosY(targetRect.anchoredPosition.y - dropDistance, dropDuration)
            .SetEase(dropEase)
            .WithCancellation(token);
    }

    // タイトルに戻ってきた時のリセット処理（Coordinatorから呼ばれる）
    public void ResetMenuPositions()
    {
        var buttons = GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            btn.interactable = true; // ボタンを再び押せるように復活
            var rect = btn.GetComponent<RectTransform>();

            // 記憶しておいた初期Y座標に戻す
            if (originalYPositions.ContainsKey(rect))
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, originalYPositions[rect]);
            }
        }
    }

    // ★追加：デフォルトのボタンを選択状態にするメソッド
    public async UniTask SelectDefaultButtonAsync() // 名前をAsyncに変更
    {
        if (defaultSelectedButton != null)
        {
            // ★重要：1フレーム待つことで、Unityに「UIがアクティブになったこと」を認識させる
            await UniTask.Yield(PlayerLoopTiming.Update);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
        }
    }
    private void DisableAllButtons()
    {
        var buttons = GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            btn.interactable = false;
        }
    }
}