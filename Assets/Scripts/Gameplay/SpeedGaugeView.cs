using UnityEngine;
using UnityEngine.UI;

public class SpeedGaugeView : MonoBehaviour
{
    [SerializeField] private Image yellowGauge;
    [SerializeField] private Image redGauge;

    // 定数としてしきい値を定義（ScoreCalculatorと共有するのが望ましい）
    public const float YellowThreshold = 0.70f;
    public const float RedThreshold = 0.00f;

    /// <summary>
    /// 残り時間の割合（0.0〜1.0）を受け取ってゲージを更新する
    /// </summary>
    public void UpdateVisual(float ratio)
    {
        // 黄色の区間
        yellowGauge.fillAmount = Mathf.InverseLerp(YellowThreshold, 1.0f, ratio);

        // 赤色の区間
        redGauge.fillAmount = Mathf.InverseLerp(RedThreshold, YellowThreshold, ratio);
    }
}