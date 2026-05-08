using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("ステージ表示情報")]
    public string stageName = "新しいステージ";

    [TextArea(3, 5)]
    public string description = "ステージの説明文をここに入力します。";

    [Header("読み込み用データID（将来用）")]
    [Tooltip("GameSceneで読み込む授業データのIDやファイル名")]
    public string lessonDataId;

    [Tooltip("GameSceneで読み込むクイズデータのIDやファイル名")]
    public string quizDataId;
}