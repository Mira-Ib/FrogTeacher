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

    // 生成オブジェクトとグループ名をセットで管理するためのクラス
    private class SpawnedItem
    {
        public string groupName;
        public GameObject gameObject;
    }
    private List<SpawnedItem> _spawnedItems = new List<SpawnedItem>();
    
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

            // ★グループ名と一緒にリストへ保持
            _spawnedItems.Add(new SpawnedItem
            {
                groupName = itemData.groupName,
                gameObject = spawnedObj
            });

            RectTransform rect = spawnedObj.GetComponent<RectTransform>();
            rect.anchoredPosition = itemData.anchoredPosition;

            if (itemData.itemType == BoardItemType.Text)
            {
                TextMeshProRuby rubyComponent = spawnedObj.GetComponent<TextMeshProRuby>();
                TextMeshProUGUI tmpComponent = spawnedObj.GetComponent<TextMeshProUGUI>();

                if (rubyComponent != null)
                {
                    rubyComponent.Text = itemData.textContent;
                }

                if (tmpComponent != null)
                {
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

    /// <summary>
    /// 指定されたグループ名のアイテムのみを消去する
    /// </summary>
    public void RemoveGroups(string[] targetGroups)
    {
        if (targetGroups == null || targetGroups.Length == 0) return;

        var targetSet = new HashSet<string>(targetGroups);

        for (int i = _spawnedItems.Count - 1; i >= 0; i--)
        {
            if (targetSet.Contains(_spawnedItems[i].groupName))
            {
                if (_spawnedItems[i].gameObject != null)
                {
                    Destroy(_spawnedItems[i].gameObject);
                }
                _spawnedItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 指定されたグループ名「以外」のアイテムを消去する（＝指定グループを残す）
    /// </summary>
    public void ClearExceptGroups(string[] keepGroups)
    {
        if (keepGroups == null || keepGroups.Length == 0)
        {
            ClearBoard();
            return;
        }

        var keepSet = new HashSet<string>(keepGroups);

        for (int i = _spawnedItems.Count - 1; i >= 0; i--)
        {
            if (!keepSet.Contains(_spawnedItems[i].groupName))
            {
                if (_spawnedItems[i].gameObject != null)
                {
                    Destroy(_spawnedItems[i].gameObject);
                }
                _spawnedItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 黒板上の全アイテムを消去する
    /// </summary>
    public void ClearBoard()
    {
        foreach (var item in _spawnedItems)
        {
            if (item.gameObject != null) Destroy(item.gameObject);
        }
        _spawnedItems.Clear();
    }

    public async UniTask HideAndClearBoardAsync(CancellationToken token)
    {
        if (_spawnedItems.Count == 0) return;

        _fadeTween = _contentCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        await _fadeTween.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);

        ClearBoard();
        _contentCanvasGroup.alpha = 1f;
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