using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTweenを使用

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))] // フェード用に必須
public class StudentVoiceEffect : MonoBehaviour
{
    [Header("Appearance (出現)")]
    [SerializeField] private float targetScale = 1.0f;    // ★これを追加（初期値1.0）
    [SerializeField] private float popDuration = 0.2f;    // ぱきっと出る速度
    [SerializeField] private Ease popEase = Ease.OutBack; // 少し弾けるような動き

    [Header("Wait (静止)")]
    [SerializeField] private float displayDuration = 1.0f; // ★インスペクタで調整する表示秒数

    [Header("Fade Out (消滅)")]
    [SerializeField] private float fadeDuration = 0.5f;   // 消える速度

    private Image image;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void StartEffect()
    {
        // 1. 初期状態のセット（大きさ0、不透明度1）
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;

        // DOTweenのシーケンス作成
        Sequence seq = DOTween.Sequence();

        // 2. 【出現】ぱきっと拡大して、規定の大きさ（targetScale）へ
        // SetNativeSizeはSpawner側で実行済みなので、localScaleを変えるだけでok
        seq.Append(transform.DOScale(targetScale, popDuration).SetEase(popEase));

        // 3. 【静止】指定した秒数だけ、何もしないで待つ
        seq.AppendInterval(displayDuration);

        // 4. 【消滅】フェードアウト
        seq.Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));

        // 5. 終了後に自分自身を削除
        seq.OnComplete(() => {
            Destroy(gameObject);
        });
    }
}