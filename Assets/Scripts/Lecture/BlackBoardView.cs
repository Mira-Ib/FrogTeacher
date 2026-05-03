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

    /// <summary>
    /// 指定されたアイテム群を黒板に即座に追加する（同期処理）
    /// </summary>
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
            // アニメーションなしで即座に等倍にする
            rect.localScale = Vector3.one;

            if (itemData.itemType == BoardItemType.Text)
            {
                spawnedObj.GetComponent<TextMeshProUGUI>().text = itemData.textContent;
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
        // アニメーションがないので、今回は特に何もする必要はありません
        // （将来的に何か状態をリセットする必要が出た時のために空のメソッドとして残します）
    }
}