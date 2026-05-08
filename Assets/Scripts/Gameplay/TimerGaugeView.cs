using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class TimerGaugeView : MonoBehaviour
{
    private Image _gaugeImage;
    private Tween _gaugeTween;

    [Header("色の設定")]
    [SerializeField, Tooltip("最初の色（余裕がある時）")]
    private Color highColor = Color.green;

    [SerializeField, Tooltip("途中の色（少し焦る時）")]
    private Color mediumColor = Color.yellow;

    [SerializeField, Tooltip("後半の色（ピンチの時）")]
    private Color lowColor = Color.red;

    [Header("色が変わる基準（0.0〜1.0）")]
    [SerializeField, Range(0f, 1f), Tooltip("この割合以下になったら「途中」の色にする")]
    private float mediumThreshold = 0.5f; // デフォルトは半分(50%)

    [SerializeField, Range(0f, 1f), Tooltip("この割合以下になったら「後半」の色にする")]
    private float lowThreshold = 0.2f;  // デフォルトは残り20%

    private void Awake()
    {
        _gaugeImage = GetComponent<Image>();
        _gaugeImage.type = Image.Type.Filled;
        _gaugeImage.fillAmount = 1f;
    }

    private void OnEnable()
    {
        MusicTimer.OnTimerStarted += StartGauge;
        MusicTimer.OnGameEnded += StopGauge;
    }

    private void OnDisable()
    {
        MusicTimer.OnTimerStarted -= StartGauge;
        MusicTimer.OnGameEnded -= StopGauge;
        _gaugeTween?.Kill();
    }

    private void StartGauge(float duration)
    {
        // ゲージの量と色を満タン（初期状態）にリセット
        _gaugeImage.fillAmount = 1f;
        UpdateColor(); // ★開始時に色をリセット

        _gaugeTween?.Kill();

        // ★修正：OnUpdate() を追加して、値が減るたびに色をチェックさせる
        _gaugeTween = _gaugeImage.DOFillAmount(0f, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() => UpdateColor());
    }

    private void StopGauge()
    {
        _gaugeTween?.Kill();
        _gaugeImage.fillAmount = 0f;
        UpdateColor(); // 念のため最後も色を更新
    }

    // ==========================================
    // ★追加：現在のゲージ量を見て色を変える処理
    // ==========================================
    private void UpdateColor()
    {
        float currentFill = _gaugeImage.fillAmount;

        // 値が小さい（ピンチな）条件から順番に判定していくのがポイントです
        if (currentFill <= lowThreshold)
        {
            _gaugeImage.color = lowColor;
        }
        else if (currentFill <= mediumThreshold)
        {
            _gaugeImage.color = mediumColor;
        }
        else
        {
            _gaugeImage.color = highColor;
        }
    }
}