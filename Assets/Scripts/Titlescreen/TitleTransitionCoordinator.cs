using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TitleTransitionCoordinator : MonoBehaviour
{
    [Header("背景・演出の参照")]
    [SerializeField] private BlackboardDirector blackboardDirector;
    [SerializeField] private MaskTransitionEffect maskTransitionEffect;

    // 【未実装のためコメントアウト】
    // [SerializeField] private FrogTeacherAnimator frogTeacherAnimator;
    // [SerializeField] private TitleMenuController titleMenuController; 
    // [SerializeField] private NextScreenPanel nextScreenPanel;

    [Header("UIとキャラクターのGameObject")]
    [Tooltip("選ばれたメニューとカエル先生を保護するために参照します")]
    [SerializeField] private GameObject frogTeacherObject;

    // 黒板アニメーションのキャンセル制御用
    private CancellationTokenSource backgroundAnimationCts;
    private bool isTransitioning = false;

    private void Start()
    {
        // 起動時に黒板のループアニメーションを開始
        backgroundAnimationCts = new CancellationTokenSource();
        blackboardDirector.PlayBlackboardLoopAsync(backgroundAnimationCts.Token).Forget();
    }

    /// <summary>
    /// UIの選択肢が決定された時に呼ばれるエントリーポイント
    /// </summary>
    public void OnMenuSelected(GameObject selectedMenuObject)
    {
        // 多重押し防止
        if (isTransitioning) return;
        isTransitioning = true;

        // シーケンスを開始
        PlayTransitionSequenceAsync(selectedMenuObject).Forget();
    }

    private async UniTask PlayTransitionSequenceAsync(GameObject selectedMenuObject)
    {
        // このGameObjectが破棄された時に安全に止めるためのトークン
        var sequenceToken = this.GetCancellationTokenOnDestroy();

        // --- 0. 背景のループアニメーションを停止 ---
        if (backgroundAnimationCts != null)
        {
            backgroundAnimationCts.Cancel();
            backgroundAnimationCts.Dispose();
            backgroundAnimationCts = null;
        }

        // --- 1. 背景の消去 ---
        // 選んだ選択肢とカエル先生「以外」をマスクで左から右へ消去
        await maskTransitionEffect.EraseBackgroundExceptAsync(selectedMenuObject, frogTeacherObject, sequenceToken);

        // ==========================================
        // 以下、未実装クラスの演出処理は一旦コメントアウト
        // ==========================================
        /*
        // --- 2. カエル先生の退場 ---
        // その場で飛び上がり、右へ等速で退場するまで待機
        await frogTeacherAnimator.JumpAndExitAsync(sequenceToken);

        // --- 3. 画面の落下（同時実行） ---
        // 選択肢の落下と、次画面（ステージ選択等）の降下を「同時に」実行する
        await UniTask.WhenAll(
            titleMenuController.DropSelectedMenuAsync(selectedMenuObject, sequenceToken),
            nextScreenPanel.DropInAsync(sequenceToken)
        );
        */

        // --- 4. 完了後の処理 ---
        // 動作確認用のログ
        Debug.Log("背景の消去まで完了しました。以降の演出は未実装です。");
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄される際の安全なクリーンアップ
        if (backgroundAnimationCts != null)
        {
            backgroundAnimationCts.Cancel();
            backgroundAnimationCts.Dispose();
        }
    }
}