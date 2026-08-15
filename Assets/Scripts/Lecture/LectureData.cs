using UnityEngine;

// 黒板に表示する要素の種類
public enum BoardItemType
{
    Text,
    Image
}

// 黒板に追加する1つの要素のデータ
[System.Serializable]
public class BlackboardItemData
{
    [Header("グループ名（例: Title, Content_1, Content_2 など）")]
    public string groupName = "Default";
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


public enum BoardClearType
{
    None,            // 消去なし（どんどん追記する）
    ClearAll,        // 黒板全体を一括削除
    RemoveGroup,     // 指定したグループのみ削除
    KeepGroup        // 指定したグループだけ残して、他を削除
}

[System.Serializable]
public class LectureStep
{
    [Header("黒板のリセット設定")]
    public BoardClearType clearType = BoardClearType.None;
    
    [Tooltip("RemoveGroup または KeepGroup の対象とするグループ名一覧")]
    public string[] targetGroups;

    [TextArea(2, 3)]
    public string dialogueText;    // カエル先生のセリフ
    public bool isFrogHandLowered;  // 先生の手を上げるか下ろすか
    public bool playChalkSound;    // このセリフでチョークの音を鳴らすか

    [Header("このステップで黒板に追加する要素")]
    public BlackboardItemData[] newBoardItems; // 同時に複数表示できるように配列にする
}

[CreateAssetMenu(fileName = "NewLectureData", menuName = "Game/Lecture Data")]
public class LectureData : ScriptableObject
{
    public LectureStep[] steps;
}