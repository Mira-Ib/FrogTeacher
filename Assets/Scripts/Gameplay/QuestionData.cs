using UnityEngine;

// 問題の種類を定義
public enum QuestionType
{
    Normal,      // 通常
    GoodQuestion, // 良い質問
    Taboo        // 禁忌肢
}

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Quiz/QuestionData")]
public class QuestionData : ScriptableObject
{
    [Header("基本データ")]
    [TextArea(3, 5)] public string questionText;
    public bool isTrueKero;

    [Header("出現条件")]
    public bool isFirstFixed;   // 1問目固定
    public bool isLateGameOnly; // 後半（51問目以降）限定フラグ

    [Header("問題の属性")]
    public QuestionType questionType; // ここで種類を選択！

    [Header("演出設定")]
    public float fontSize = 50f;
    public bool useAutoSizing = false;
}