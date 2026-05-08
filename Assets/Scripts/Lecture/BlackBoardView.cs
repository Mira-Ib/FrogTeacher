using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMP_Ruby; // ★追加：TMP_Rubyの名前空間
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

    private Tween _fadeTween;

    private void Awake()
    {
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
            GameObject prefabToSpawn = itemData.itemType == BoardItemType.Text ? textPrefab : imagePrefab;
            GameObject spawnedObj = Instantiate(prefabToSpawn, boardContentParent);
            _spawnedItems.Add(spawnedObj);

            RectTransform rect = spawnedObj.GetComponent<RectTransform>();
            rect.anchoredPosition = itemData.anchoredPosition;

            if (itemData.itemType == BoardItemType.Text)
            {
                // ==================================================
                // ★修正：ルビ変換用(Ruby)と描画用(UGUI)を両方取得する
                // ==================================================
                TextMeshProRuby rubyComponent = spawnedObj.GetComponent<TextMeshProRuby>();
                TextMeshProUGUI tmpComponent = spawnedObj.GetComponent<TextMeshProUGUI>();

                if (rubyComponent != null)
                {
                    // テキストの内容はルビ変換コンポーネントに任せる
                    // ※アセットのバージョンによっては .Text と大文字の場合があります。エラーが出たら直してください。
                    rubyComponent.Text = itemData.textContent;
                }
                else
                {
                    Debug.LogWarning("黒板用のテキストプレハブに TextMeshProRuby がアタッチされていません！");
                }

                if (tmpComponent != null)
                {
                    // 色やフォントサイズなどの「見た目」は、実体の描画コンポーネントに直接指示する
                    if (itemData.fontSize > 0f) tmpComponent.fontSize = itemData.fontSize;
                    if (itemData.overrideColor) tmpComponent.color = itemData.textColor;
                }

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

    public async UniTask HideAndClearBoardAsync(CancellationToken token)
    {
        if (_spawnedItems.Count == 0) return;

        _fadeTween = _contentCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        await _fadeTween.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);

        ClearBoard();
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
        if (_fadeTween != null && _fadeTween.IsActive())
        {
            _fadeTween.Kill();
        }

        ClearBoard();
        _contentCanvasGroup.alpha = 1f;
    }
}