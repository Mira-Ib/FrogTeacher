using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("ステージ表示情報")]
    public string stageName = "新しいステージ";

    [TextArea(3, 5)]
    public string description = "ステージの説明文をここに入力します。";

    [Header("ゲーム本編のデータ")]
    public LectureData lectureData;
    // ★追加：このステージで使うクイズの100問セット
    public QuizData quizData;
}