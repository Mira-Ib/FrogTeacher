using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class TitleTransitionCoordinator : MonoBehaviour
{
    [Header("背景・演出の参照")]
    [SerializeField] private BlackboardDirector blackboardDirector;
    [SerializeField] private MaskTransitionEffect maskTransitionEffect;
    [SerializeField] private FrogTeacherAnimator frogTeacherAnimator;
    [SerializeField] private TitleMenuController titleMenuController;

    [Header("タイトル画面の制御")]
    [Tooltip("タイトル画面の親（MaskedContainer）")]
    [SerializeField] private GameObject titleMaskedContainer;
    [Tooltip("決定演出で避難させる用")]
    [SerializeField] private GameObject frogTeacherObject;

    // ★追加：タイトル画面に戻ってきた時に、最初に矢印を合わせたいボタン
    [Tooltip("タイトル復帰時に最初にフォーカスを当てるボタン")]
    [SerializeField] private GameObject _firstButtonObject;
    [Tooltip("タイトル復帰時に、上から降らせたいオブジェクトをここに登録します（MenuListやFrogTeacherなど）")]
    [SerializeField] private List<RectTransform> titleDropInElements;

    private CancellationTokenSource backgroundAnimationCts;
    private bool isTransitioning = false;

    // ★追加：起動時の正しいY座標を記憶するための辞書
    private Dictionary<RectTransform, float> _originalYPositions = new Dictionary<RectTransform, float>();

    private void Start()
    {
        // ★追加：ゲームが起動した瞬間の、各オブジェクトの正しいY座標を記憶しておく
        foreach (var element in titleDropInElements)
        {
            if (element != null)
            {
                _originalYPositions[element] = element.anchoredPosition.y;
            }
        }
    }

    public void StartBackgroundAnimation()
    {
        if (backgroundAnimationCts != null) return;
        backgroundAnimationCts = new CancellationTokenSource();
        blackboardDirector.PlayBlackboardLoopAsync(backgroundAnimationCts.Token).Forget();
    }

    // === 行きの演出（タイトル → 各画面） ===
    public void OnMenuSelected(GameObject selectedMenuObject, NextScreenPanel targetPanel)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayTransitionSequenceAsync(selectedMenuObject, targetPanel).Forget();
    }

    private async UniTask PlayTransitionSequenceAsync(GameObject selectedMenuObject, NextScreenPanel targetPanel)
    {
        var sequenceToken = this.GetCancellationTokenOnDestroy();

        backgroundAnimationCts?.Cancel();
        backgroundAnimationCts?.Dispose();
        backgroundAnimationCts = null;

        maskTransitionEffect.ProtectObjects(selectedMenuObject, frogTeacherObject);
        await UniTask.WhenAll(
            maskTransitionEffect.WipeOutAsync(1.0f, sequenceToken),
            frogTeacherAnimator.JumpAndExitAsync(sequenceToken)
        );

        targetPanel.ResetToStartPos();
        await UniTask.WhenAll(
            titleMenuController.DropSelectedMenuAsync(selectedMenuObject, sequenceToken),
            targetPanel.DropInAsync(sequenceToken)
        );

        titleMaskedContainer.SetActive(false);
        isTransitioning = false;
    }

    // === 帰りの演出（各画面 → タイトル） ===
    public void ReturnToTitle(NextScreenPanel currentPanel)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayReturnSequenceAsync(currentPanel).Forget();
    }

    private async UniTask PlayReturnSequenceAsync(NextScreenPanel currentPanel)
    {
        var sequenceToken = this.GetCancellationTokenOnDestroy();

        // 1. 現在の画面をマスク消去
        await currentPanel.WipeOutAsync(sequenceToken);

        // 2. タイトル画面の準備
        PrepareForReturn();

        // 3. タイトルを一斉に降らせるタスクを開始
        await ExecuteDropInAsync(sequenceToken);

        // 4. 黒板アニメーション再開
        StartBackgroundAnimation();

        // ==========================================
        // ★変更：指定したボタンに確実にフォーカスを当てる
        // ==========================================
        if (UnityEngine.EventSystems.EventSystem.current != null && _firstButtonObject != null)
        {
            // 一旦フォーカスを完全にクリアする（古い選択状態をリセット）
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            // インスペクタで指定したボタンを選択状態にする
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_firstButtonObject);
        }
        // ==========================================

        isTransitioning = false;
    }

    public void PrepareForReturn()
    {
        maskTransitionEffect.ResetMaskAndParents();
        ResetTitleState();

        foreach (var element in titleDropInElements)
        {
            element.anchoredPosition = new Vector2(element.anchoredPosition.x, 1500f);
        }

        titleMaskedContainer.SetActive(true);
    }

    public async UniTask ExecuteDropInAsync(CancellationToken token)
    {
        var dropTasks = titleDropInElements.Select(element =>
        {
            // ★変更：0f 固定ではなく、辞書に記憶しておいた正しいY座標を取得する
            float targetY = _originalYPositions.TryGetValue(element, out float y) ? y : 0f;

            return element.DOAnchorPosY(targetY, 0.8f)
                .SetEase(Ease.OutBack)
                .WithCancellation(token);
        });

        await UniTask.WhenAll(dropTasks);
    }

    public void ResetTitleState()
    {
        titleMenuController.ResetMenuPositions();
        frogTeacherAnimator.ResetPosition();
    }
}