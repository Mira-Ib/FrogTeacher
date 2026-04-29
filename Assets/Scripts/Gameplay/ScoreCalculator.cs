using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    private int comboCount = 0;
    private const int MaxBaseScore = 10;

    public int CalculateScore(float ratio)
    {
        comboCount++;
        return GetCurrentBaseScore() * GetCurrentMultiplier(ratio);
    }

    public int GetCurrentBaseScore() => Mathf.Min(comboCount, MaxBaseScore);
    public int GetCurrentComboCount() => comboCount;

    public int GetCurrentMultiplier(float ratio)
    {
        if (ratio > SpeedGaugeView.YellowThreshold) return 3;
        if (ratio > SpeedGaugeView.RedThreshold) return 2;
        return 1;
    }

    public void ResetCombo() => comboCount = 0;
}