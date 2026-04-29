using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreEffectItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI multiText;
    [SerializeField] private TextMeshProUGUI totalText;

    private void Awake()
    {
        comboText.transform.localScale = Vector3.zero;
        multiText.transform.localScale = Vector3.zero;
        totalText.transform.localScale = Vector3.zero;
    }

    public void PlayCorrect(QuizResultData data, Vector3 cPos, Vector3 mPos, Vector3 tPos, ScoreEffectSpawner s)
    {
        comboText.text = $"{data.comboCount} COMBO";
        if(data.multiplier == 3)
        {
            multiText.text = "Excellent!";
        }
        else if(data.multiplier == 2)
        {
            multiText.text = "Great!";
        }
        else
        {
            multiText.text = "Good!";
        }
        totalText.text = $"+{data.totalScore}";
        totalText.color = Color.green;

        comboText.rectTransform.position = cPos;
        multiText.rectTransform.position = mPos;
        totalText.rectTransform.position = tPos;

        Sequence seq = DOTween.Sequence();

        // 1. コンボ、倍率、スコアが同時に拡大出現
        seq.Append(comboText.transform.DOScale(1.0f, s.appearDuration).SetEase(s.appearEase));
        seq.Join(multiText.transform.DOScale(1.0f, s.appearDuration).SetEase(s.appearEase));
        seq.Join(totalText.transform.DOScale(1.0f, s.appearDuration).SetEase(s.appearEase));

        // 2. 一定時間表示
        seq.AppendInterval(s.displayDuration);

        // 3. 退場（コンボと倍率はフェード、合計スコアだけ右にスライド）
        seq.Append(totalText.transform.DOMoveX(totalText.transform.position.x + s.slideDistance, s.slideDuration).SetEase(s.slideEase));
        seq.Append(comboText.DOFade(0, 0.2f));
        seq.Join(multiText.DOFade(0, 0.2f));

        // 4. アニメーション終了後にプレハブを破棄
        seq.OnComplete(() => Destroy(gameObject));
    }

    public void PlayWrong(Vector3 wPos, ScoreEffectSpawner s)
    {
        totalText.text = "-10";
        totalText.color = Color.red;
        totalText.rectTransform.position = wPos;

        Sequence seq = DOTween.Sequence();

        // 単に合計スコアを拡大表示
        seq.Append(totalText.transform.DOScale(1.0f, s.appearDuration).SetEase(s.appearEase));
        seq.AppendInterval(0.4f);

        // 同様に右スライド退場
        seq.Append(transform.DOMoveX(transform.position.x + s.slideDistance, s.slideDuration).SetEase(s.slideEase));

        seq.OnComplete(() => Destroy(gameObject));
    }
}