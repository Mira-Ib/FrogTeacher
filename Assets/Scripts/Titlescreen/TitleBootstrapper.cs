using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ★追加：EventSystemを操作するために必要
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class TitleBootstrapper : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private CanvasGroup bootCanvasGroup;
    [SerializeField] private CanvasGroup logoCanvasGroup;

    [Header("システム連携")]
    [Tooltip("アニメーション開始の合図を送るためのCoordinator")]
    [SerializeField] private TitleTransitionCoordinator coordinator;

    // ★追加：起動直後にフォーカスを当てるボタン
    [Tooltip("起動演出後に最初にフォーカスを当てるボタン")]
    [SerializeField] private GameObject firstButtonObject;

    [Header("演出設定")]
    [SerializeField] private float waitBeforeFadeIn = 0.5f;
    [SerializeField] private float logoFadeDuration = 1.0f;
    [SerializeField] private float logoDisplayDuration = 1.5f;
    [SerializeField] private float screenFadeDuration = 1.0f;

    [Header("サウンド設定")]
    [SerializeField] private BGM titleBGM = BGM.Title;

    private void Awake()
    {
        if (bootCanvasGroup == null) bootCanvasGroup = GetComponent<CanvasGroup>();

        bootCanvasGroup.alpha = 1f;
        logoCanvasGroup.alpha = 0f;
        bootCanvasGroup.blocksRaycasts = true;
    }

    private void Start()
    {
        PlayBootSequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask PlayBootSequenceAsync(CancellationToken token)
    {
        // 1. 起動直後の待機
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitBeforeFadeIn), cancellationToken: token);

        // 2. ロゴフェードイン
        await logoCanvasGroup.DOFade(1f, logoFadeDuration).WithCancellation(token);

        // 3. ロゴ表示キープ
        await UniTask.Delay(System.TimeSpan.FromSeconds(logoDisplayDuration), cancellationToken: token);

        // 4. ロゴフェードアウト
        await logoCanvasGroup.DOFade(0f, logoFadeDuration).WithCancellation(token);

        // 5. 黒い背景全体をフェードアウトして、タイトル画面を見せる（明転）
        await bootCanvasGroup.DOFade(0f, screenFadeDuration).WithCancellation(token);

        // ==================================================
        // ★ここから下が、明転し終わった瞬間に「同時」に実行されます
        // ==================================================

        // 6. BGMの再生開始！
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(titleBGM, true);
        }

        // 7. 黒板アニメーションの開始合図を送る！
        if (coordinator != null)
        {
            coordinator.StartBackgroundAnimation();
        }

        // ★追加 8. 最初のボタンにフォーカスを当てる
        if (EventSystem.current != null && firstButtonObject != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButtonObject);
        }

        // 9. 演出完了（非アクティブ化）
        bootCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}