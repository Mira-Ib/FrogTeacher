using UnityEngine;

// 黒板に表示する要素の種類
public enum BoardItemType
{
    Text,
    Image
}

// 黒板に追加する1つの要素のデータ
[System.Serializable]
public struct BlackboardItemData
{
    public BoardItemType itemType;

    [Header("Text要素の場合")]
    [TextArea(1, 3)]
    public string textContent;
    public float fontSize; // ←★ここを追加！

    [Header("テキスト色設定")]
    [Tooltip("チェックを入れると下の色が適用されます（チェックなしならプレハブのまま）")]
    public bool overrideColor;
    public Color textColor;

    [Header("Image要素の場合")]
    public Sprite imageContent;
    [Tooltip("画像の表示倍率（0のままだと自動的に1倍になります）")]
    public float imageScale;

    [Header("レイアウト設定")]
    public Vector2 anchoredPosition; // 黒板の中心からの相対座標など
    // 必要に応じてスケールやフォントサイズなどのパラメータを追加できます
}

[System.Serializable]
public struct LectureStep
{
    [TextArea(2, 3)]
    public string dialogueText;    // カエル先生のセリフ
    public bool isFrogHandLowered;  // 先生の手を上げるか下ろすか
    public bool playChalkSound;    // このセリフでチョークの音を鳴らすか

    [Header("このステップで黒板に追加する要素")]
    public BlackboardItemData[] newBoardItems; // 同時に複数表示できるように配列にする
}

[CreateAssetMenu(fileName = "NewLectureData", menuName = "Lecture/Lecture Data")]
public class LectureData : ScriptableObject
{
    public LectureStep[] steps;
}