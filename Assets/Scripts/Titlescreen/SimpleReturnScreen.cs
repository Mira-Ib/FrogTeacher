using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class SimpleReturnScreen : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private NextScreenPanel myPanel;
    [SerializeField] private TitleTransitionCoordinator coordinator;
    [SerializeField] private MaskTransitionEffect maskEffect;

    // 元のY座標を記憶する変数
    private float _defaultPosY;

    private void Awake()
    {
        // 起動時に元の高さを記憶しておく
        if (backButton != null)
        {
            _defaultPosY = backButton.GetComponent<RectTransform>().anchoredPosition.y;
        }
    }

    private void Start()
    {
        // ボタンが押されたら、非同期メソッドを呼び出す
        backButton.onClick.AddListener(() => OnBackButtonClickedAsync().Forget());
    }

    private async UniTaskVoid OnBackButtonClickedAsync()
    {
        // 1. ボタン連打防止
        backButton.interactable = false;

        // 2. 戻るボタンを SafeContainer へ避難させる
        maskEffect.ProtectObjects(backButton.gameObject);

        // 3. ボタンを落下させるアニメーション（タスク化）
        RectTransform btnRect = backButton.GetComponent<RectTransform>();
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        UniTask buttonDropTask = btnRect.DOAnchorPosY(_defaultPosY - 1000f, 0.4f)
            .SetEase(Ease.InQuad)
            .WithCancellation(token);

        // 4. コーディネーターにお任せ！（落下アニメーションを一緒に渡して完全同期）
        // ※マスク消去 → タイトル降下＆ボタン落下 → 黒板再開 → フォーカス が全自動で行われます
        await coordinator.ReturnToTitleAsync(myPanel, buttonDropTask);

        // 5. お掃除（SafeContainerから元の階層・元の座標へ戻す）
        maskEffect.ResetMaskAndParents();
        btnRect.anchoredPosition = new Vector2(btnRect.anchoredPosition.x, _defaultPosY);

        Debug.Log("クレジット画面からタイトルへの遷移が完了しました");
    }

    // 画面が開くたびにボタンを復活させる
    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.interactable = true;
        }
    }
}