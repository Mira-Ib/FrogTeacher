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

            // 一旦、基本のスケールを1倍に設定しておく
            rect.localScale = Vector3.one;

            if (itemData.itemType == BoardItemType.Text)
            {
                // (テキストの処理...変更なし)
                TextMeshProUGUI textComponent = spawnedObj.GetComponent<TextMeshProUGUI>();
                textComponent.text = itemData.textContent;

                if (itemData.fontSize > 0f)
                {
                    textComponent.fontSize = itemData.fontSize;
                }

                if (itemData.overrideColor)
                {
                    textComponent.color = itemData.textColor;
                }
            }
            else if (itemData.itemType == BoardItemType.Image)
            {
                // 画像をセット
                spawnedObj.GetComponent<Image>().sprite = itemData.imageContent;

                // ★追加：画像スケールの適用
                // 0より大きければその倍率を、0（設定忘れ）なら1倍を適用する
                float targetScale = itemData.imageScale > 0f ? itemData.imageScale : 1f;
                rect.localScale = new Vector3(targetScale, targetScale, 1f);
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