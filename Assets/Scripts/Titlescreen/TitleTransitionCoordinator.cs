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
    [SerializeField] private GameObject titleMaskedContainer;
    [SerializeField] private GameObject frogTeacherObject;
    [SerializeField] private List<RectTransform> titleDropInElements;

    private CancellationTokenSource backgroundAnimationCts;
    private bool isTransitioning = false;
    private Dictionary<RectTransform, float> _originalYPositions = new Dictionary<RectTransform, float>();

    // ★修正：Startより早いAwakeで初期位置を確実に記憶する
    private void Awake()
    {
        foreach (var element in titleDropInElements)
        {
            if (element != null)
            {
                _originalYPositions[element] = element.anchoredPosition.y;
            }
        }
    }

    // === 背景アニメーションの制御（変更なし） ===
    public void StartBackgroundAnimation()
    {
        if (backgroundAnimationCts != null) return;
        backgroundAnimationCts = new CancellationTokenSource();
        blackboardDirector.PlayBlackboardLoopAsync(backgroundAnimationCts.Token).Forget();
    }

    private void StopBackgroundAnimation()
    {
        if (backgroundAnimationCts != null)
        {
            backgroundAnimationCts.Cancel();
            backgroundAnimationCts.Dispose();
            backgroundAnimationCts = null;
        }
        blackboardDirector.ResetAllToBlank();
    }

    // === 行きの演出（変更なし） ===
    public void OnMenuSelected(GameObject selectedMenuObject, NextScreenPanel targetPanel)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayTransitionSequenceAsync(selectedMenuObject, targetPanel).Forget();
    }

    private async UniTask PlayTransitionSequenceAsync(GameObject selectedMenuObject, NextScreenPanel targetPanel)
    {
        var sequenceToken = this.GetCancellationTokenOnDestroy();

        StopBackgroundAnimation();
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

    // === 帰りの演出（★ここがアーキテクチャの要） ===

    /// <summary>
    /// タイトル画面への復帰シーケンスを完全自動で実行します。
    /// 外部で同時に実行したいアニメーションがある場合は extraParallelTask に渡すことができます。
    /// </summary>
    public async UniTask ReturnToTitleAsync(NextScreenPanel currentPanel, UniTask extraParallelTask = default)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        var sequenceToken = this.GetCancellationTokenOnDestroy();

        // 1. 現在の画面を消去
        await currentPanel.WipeOutAsync(sequenceToken);

        // 2. タイトルの状態をリセットし、降下準備をする
        PrepareForReturn();
        await titleMenuController.SelectDefaultButtonAsync();

        // 3. 指定した要素を一斉に降らせる ＋ 外部のタスク（ボタン落下など）を【完全に同期】させる
        await UniTask.WhenAll(
            ExecuteDropInAsync(sequenceToken),
            extraParallelTask
        );

        // 4. 黒板アニメーション再開
        StartBackgroundAnimation();

        // 5. タイトルが復帰したらフォーカスを当てる（→2に移動）
        

        isTransitioning = false;
    }

    // === 内部処理用のヘルパーメソッド（★外部から呼べないように private に隠蔽！） ===

    private void PrepareForReturn()
    {
        maskTransitionEffect.ResetMaskAndParents();
        titleMenuController.ResetMenuPositions();
        frogTeacherAnimator.ResetPosition();

        foreach (var element in titleDropInElements)
        {
            if (element != null)
            {
                element.anchoredPosition = new Vector2(element.anchoredPosition.x, 1500f);
            }
        }

        titleMaskedContainer.SetActive(true);
    }

    private async UniTask ExecuteDropInAsync(CancellationToken token)
    {
        var dropTasks = titleDropInElements.Select(element =>
        {
            if (element == null) return UniTask.CompletedTask;
            float targetY = _originalYPositions.TryGetValue(element, out float y) ? y : 0f;
            return element.DOAnchorPosY(targetY, 0.8f)
                .SetEase(Ease.OutBack)
                .WithCancellation(token);
        });

        await UniTask.WhenAll(dropTasks);
    }
}