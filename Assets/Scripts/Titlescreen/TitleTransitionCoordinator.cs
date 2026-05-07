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

    [Tooltip("タイトル復帰時に、上から降らせたいオブジェクトをここに登録します（MenuListやFrogTeacherなど）")]
    [SerializeField] private List<RectTransform> titleDropInElements;

    private CancellationTokenSource backgroundAnimationCts;
    private bool isTransitioning = false;

    // 起動時の正しいY座標を記憶するための辞書
    private Dictionary<RectTransform, float> _originalYPositions = new Dictionary<RectTransform, float>();

    private void Start()
    {
        // ゲームが起動した瞬間の、各オブジェクトの正しいY座標を記憶しておく
        foreach (var element in titleDropInElements)
        {
            if (element != null)
            {
                _originalYPositions[element] = element.anchoredPosition.y;
            }
        }
    }

    // === 背景アニメーションの制御 ===
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
        // アニメーションを停止した瞬間に、画像を非表示にしてまっさらな黒板にする
        blackboardDirector.ResetAllToBlank();
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

        // 1. 背景アニメーション停止とまっさらな状態へのリセット
        StopBackgroundAnimation();

        // 2. 保護とマスク消去・カエル退場を同時実行
        maskTransitionEffect.ProtectObjects(selectedMenuObject, frogTeacherObject);
        await UniTask.WhenAll(
            maskTransitionEffect.WipeOutAsync(1.0f, sequenceToken),
            frogTeacherAnimator.JumpAndExitAsync(sequenceToken)
        );

        // 3. メニュー落下 ＆ 次画面の降下を同時実行
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

        // 1. 現在の画面を左から右へマスク消去
        await currentPanel.WipeOutAsync(sequenceToken);

        // 2. タイトルの状態をリセットし、降下準備をする
        PrepareForReturn();

        // 3. 指定した要素を一斉に降らせる（記憶した正しいY座標へ）
        await ExecuteDropInAsync(sequenceToken);

        // 4. 黒板アニメーション再開
        StartBackgroundAnimation();

        // 5. タイトルが復帰したらフォーカスを当てる
        // 警告が出ないよう、1フレーム待機が含まれる処理を await で待つ
        await titleMenuController.SelectDefaultButtonAsync();

        isTransitioning = false;
    }

    // === 内部処理用のヘルパーメソッド ===
    public void PrepareForReturn()
    {
        // 避難したUIの帰還とマスクの全開
        maskTransitionEffect.ResetMaskAndParents();

        // カエルとメニューの位置リセット
        titleMenuController.ResetMenuPositions();
        frogTeacherAnimator.ResetPosition();

        // 降ってくる要素を上空へ配置
        foreach (var element in titleDropInElements)
        {
            if (element != null)
            {
                element.anchoredPosition = new Vector2(element.anchoredPosition.x, 1500f);
            }
        }

        titleMaskedContainer.SetActive(true);
    }

    public async UniTask ExecuteDropInAsync(CancellationToken token)
    {
        var dropTasks = titleDropInElements.Select(element =>
        {
            if (element == null) return UniTask.CompletedTask;

            // 0f固定ではなく、辞書に記憶しておいた正しいY座標を取得して降下させる
            float targetY = _originalYPositions.TryGetValue(element, out float y) ? y : 0f;
            return element.DOAnchorPosY(targetY, 0.8f)
                .SetEase(Ease.OutBack)
                .WithCancellation(token);
        });

        await UniTask.WhenAll(dropTasks);
    }
}