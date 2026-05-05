using UnityEngine;

[CreateAssetMenu(fileName = "NewFormulaData", menuName = "GameData/FormulaData")]
public class FormulaData : ScriptableObject
{
    public bool isMainFormula; // 1+1=3 かどうかの判定用
    public Sprite[] frames;    // 11枚の画像
}