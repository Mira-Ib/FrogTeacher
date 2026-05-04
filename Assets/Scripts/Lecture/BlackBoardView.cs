using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BlackboardView : MonoBehaviour, ILecturePlayable
{
    [Header("生成用プレハブ")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject imagePrefab;

    [Header("黒板の親オブジェクト")]
    [SerializeField] private RectTransform boardContentParent;

    private List<GameObject> _spawnedItems = new List<GameObject>();

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
            rect.localScale = Vector3.one;

            if (itemData.itemType == BoardItemType.Text)
            {
                // TextMeshProUGUIコンポーネントを取得
                TextMeshProUGUI textComponent = spawnedObj.GetComponent<TextMeshProUGUI>();

                // テキスト内容をセット
                textComponent.text = itemData.textContent;

                // ★追加：フォントサイズが0より大きい場合のみ上書きする
                if (itemData.fontSize > 0f)
                {
                    textComponent.fontSize = itemData.fontSize;
                }

                // ★追加：色の上書き（チェックが入っている場合のみ）
                if (itemData.overrideColor)
                {
                    textComponent.color = itemData.textColor;
                }
            }
            else if (itemData.itemType == BoardItemType.Image)
            {
                spawnedObj.GetComponent<Image>().sprite = itemData.imageContent;
            }
        }
    }

    public void ClearBoard()
    {
        foreach (var item in _spawnedItems)
        {
            Destroy(item);
        }
        _spawnedItems.Clear();
    }

    public void FastForward()
    {
        // アニメーションなし
    }
}