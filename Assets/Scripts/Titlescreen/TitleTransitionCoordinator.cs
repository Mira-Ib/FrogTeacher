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

    private void Start()
    {
        StartBackgroundAnimation();
    }

    private void StartBackgroundAnimation()
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

        // 1. アニメーション停止とマスク消去
        backgroundAnimationCts?.Cancel();
        backgroundAnimationCts?.Dispose();
        backgroundAnimationCts = null;
        await maskTransitionEffect.EraseBackgroundExceptAsync(selectedMenuObject, frogTeacherObject, sequenceToken);

        // 2. カエル退場
        await frogTeacherAnimator.JumpAndExitAsync(sequenceToken);

        // 3. メニュー落下 ＆ 次画面が降ってくる
        targetPanel.ResetToStartPos();
        await UniTask.WhenAll(
            titleMenuController.DropSelectedMenuAsync(selectedMenuObject, sequenceToken),
            targetPanel.DropInAsync(sequenceToken)
        );

        // 行き終わったらタイトル画面自体を完全に非アクティブにする
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

        // 2. タイトルの状態をリセット（避難したUIをMaskedContainerに戻し、マスクを全開にする）
        ResetTitleState();

        // 3. インスペクタで指定した要素（MenuListなど）を上空へ移動させる
        foreach (var element in titleDropInElements)
        {
            element.anchoredPosition = new Vector2(element.anchoredPosition.x, 1500f);
        }

        // タイトル画面を表示
        titleMaskedContainer.SetActive(true);

        // 4. 指定した要素を一斉に降らせる
        var dropTasks = titleDropInElements.Select(element =>
            element.DOAnchorPosY(0f, 0.8f).SetEase(Ease.OutBack).WithCancellation(sequenceToken)
        );
        await UniTask.WhenAll(dropTasks);

        // 5. 黒板アニメーション再開
        StartBackgroundAnimation();

        isTransitioning = false;
    }

    private void ResetTitleState()
    {
        // 1. 落下させたすべての選択肢（ボタン）のY座標を初期位置に戻し、再び押せるようにする
        titleMenuController.ResetMenuPositions();

        // 2. 画面右外へ退場したカエル先生のX座標を初期位置に戻す
        frogTeacherAnimator.ResetPosition();

        // 3. 避難させていたUIを元の階層に戻し、全体のマスクを全開にする
        maskTransitionEffect.ResetMaskAndParents();
    }
}