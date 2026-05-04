using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class BlackboardView : MonoBehaviour, ILecturePlayable
{
    [Header("生成用プレハブ")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject imagePrefab;

    [Header("黒板の親オブジェクト")]
    [SerializeField] private RectTransform boardContentParent;

    [Header("フェード演出設定")]
    [SerializeField] private float fadeDuration = 0.5f; // フェードにかかる秒数

    private List<GameObject> _spawnedItems = new List<GameObject>();
    private CanvasGroup _contentCanvasGroup;

    // ★追加：フェードアニメーションを保持する変数
    private Tween _fadeTween;

    private void Awake()
    {
        // 親オブジェクトに CanvasGroup が付いていなければ自動で追加する
        _contentCanvasGroup = boardContentParent.GetComponent<CanvasGroup>();
        if (_contentCanvasGroup == null)
        {
            _contentCanvasGroup = boardContentParent.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void AddItems(BlackboardItemData[] items)
    {
        if (items == null || items.Length == 0) return;

        foreach (var itemData in items)
        {
            // (以前のままの生成処理...)
            GameObject prefabToSpawn = itemData.itemType == BoardItemType.Text ? textPrefab : imagePrefab;
            GameObject spawnedObj = Instantiate(prefabToSpawn, boardContentParent);
            _spawnedItems.Add(spawnedObj);

            RectTransform rect = spawnedObj.GetComponent<RectTransform>();
            rect.anchoredPosition = itemData.anchoredPosition;

            if (itemData.itemType == BoardItemType.Text)
            {
                TextMeshProUGUI textComponent = spawnedObj.GetComponent<TextMeshProUGUI>();
                textComponent.text = itemData.textContent;
                if (itemData.fontSize > 0f) textComponent.fontSize = itemData.fontSize;
                if (itemData.overrideColor) textComponent.color = itemData.textColor;

                // スケールを1倍に
                rect.localScale = Vector3.one;
            }
            else if (itemData.itemType == BoardItemType.Image)
            {
                spawnedObj.GetComponent<Image>().sprite = itemData.imageContent;
                float targetScale = itemData.imageScale > 0f ? itemData.imageScale : 1f;
                rect.localScale = new Vector3(targetScale, targetScale, 1f);
            }
        }
    }

    /// <summary>
    /// 黒板の中身をフェードアウトさせ、完全に消えたらDestroyする
    /// </summary>
    public async UniTask HideAndClearBoardAsync(CancellationToken token)
    {
        if (_spawnedItems.Count == 0) return;

        // 1. 親オブジェクトの透明度を0（透明）にするアニメーション
        _fadeTween = _contentCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        await _fadeTween.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);

        // 2. 完全に透明になったら中身をDestroyする
        ClearBoard();

        // 3. 次の授業（またはリトライ時）のために、親の透明度を1（不透明）に戻しておく
        _contentCanvasGroup.alpha = 1f;
    }

    public void ClearBoard()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        _spawnedItems.Clear();
    }

    public void FastForward()
    {
        // ★修正：大雑把な DOKill() をやめ、変数の生存確認をしてから Kill する
        if (_fadeTween != null && _fadeTween.IsActive())
        {
            _fadeTween.Kill();
        }

        // スキップされたら即座に中身を消去し、透明度を戻す
        ClearBoard();
        _contentCanvasGroup.alpha = 1f;
    }
}