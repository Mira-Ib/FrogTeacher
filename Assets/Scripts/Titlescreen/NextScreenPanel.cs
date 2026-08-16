using System.Threading;
using UnityEngine;
using UnityEngine.UI; // ★追加：Buttonコンポーネントを操作するために必要です
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class NextScreenPanel : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("演出の設定")]
    [SerializeField] private float startPosY = 1500f;
    [SerializeField] private float targetPosY = 0f;
    [SerializeField] private float animationDuration = 0.8f;

    [Header("マスクの参照")]
    [SerializeField] private HorizontalWipeMask wipeMask;

    [Header("UIナビゲーション")]
    [Tooltip("この画面が開ききった時に最初に選択状態にするUI（スライダーや戻るボタン等）")]
    [SerializeField] private GameObject firstSelectedElement;

    // ★追加：2回目以降の表示バグを防ぐための設定
    [Header("リセット設定")]
    [Tooltip("この画面の『決定ボタン』または『戻るボタン』を登録します")]
    [SerializeField] private RectTransform submitButton;
    private float _defaultButtonY;

    private void Awake()
    {
        EnsureInitialized();

        // ★追加：起動時のボタンの正しいY座標を記憶しておく
        if (submitButton != null)
        {
            _defaultButtonY = submitButton.anchoredPosition.y;
        }
    }

    // ★追加：画面が有効（SetActive(true)）になった瞬間に自動でリセットを実行
    private void OnEnable()
    {
        if (submitButton != null)
        {
            // 1. ボタンを元の高さに強制的に戻す（落下したままになるのを防ぐ）
            Vector2 pos = submitButton.anchoredPosition;
            pos.y = _defaultButtonY;
            submitButton.anchoredPosition = pos;

            // 2. ボタンを再び押せるようにする
            var btn = submitButton.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    private void EnsureInitialized()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public async UniTask DropInAsync(CancellationToken token)
    {
        EnsureInitialized();

        // SetActive(true) により、上記の OnEnable リセット処理が自動的に走ります
        gameObject.SetActive(true);

        if (wipeMask != null) wipeMask.ResetMask();

        // 上から降ってくるアニメーション
        await rectTransform.DOAnchorPosY(targetPosY, animationDuration)
            .SetEase(Ease.OutBack)
            .WithCancellation(token);

        // ★修正：降下アニメーションが完全に終わった後でボタンを有効化する
        if (submitButton != null)
        {
            var btn = submitButton.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }

        // アニメーションが終わったらフォーカスを当てる
        if (firstSelectedElement != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedElement);
        }
    }

    public async UniTask WipeOutAsync(CancellationToken token)
    {
        EnsureInitialized();
        if (wipeMask != null)
        {
            await wipeMask.WipeOutAsync(animationDuration, token);
        }
        gameObject.SetActive(false);
    }

    public void ResetToStartPos()
    {
        EnsureInitialized();
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startPosY);
        gameObject.SetActive(false);
    }
}