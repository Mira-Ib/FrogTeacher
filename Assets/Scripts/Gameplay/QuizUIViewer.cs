using TMP_Ruby; // 以前解決した名前空間
using TMPro;
using UnityEngine;

public class QuizUIViewer : MonoBehaviour
{
    [Header("ルビ対応メインテキスト")]
    [SerializeField] private TextMeshProRuby rubyInput;

    [Header("前後定型文（通常のTMP）")]
    [SerializeField] private TextMeshProUGUI introTextDisplay; // 「つまり…」「じゃあ…」
    [SerializeField] private TextMeshProUGUI outroTextDisplay; // 「ということですか！？」

 
    /// <summary>
    /// クイズUIのセットアップ
    /// </summary>
    /// <param name="data">問題データ</param>
    /// <param name="answeredCount">現在の回答済み数（0なら1問目）</param>
    public void SetupQuestionUI(QuestionData data, int answeredCount)
    {
        // 1. 前後の定型文を制御
        introTextDisplay.text = (answeredCount == 0) ? "つまり…" : "じゃあ…";
        outroTextDisplay.text = "ということですか！？";

        // 2. メインのTMP設定（フォントサイズなど）
        var mainText = rubyInput.GetComponent<TMP_Text>();
        mainText.enableAutoSizing = data.useAutoSizing;
        if (!data.useAutoSizing)
        {
            mainText.fontSize = data.fontSize;
        }

        // 3. ルビ付きテキストを流し込む（大文字の Text プロパティを使用）
        rubyInput.Text = data.questionText;
    }


    private void OnEnable()
    {
        // クイズ開始前（有効化時）に登録
        MusicTimer.OnGameEnded += ShowTimeUp;
    }

    private void OnDisable()
    {
        // 無効化時に解除（メモリリークやエラー防止の鉄則）
        MusicTimer.OnGameEnded -= ShowTimeUp;
    }
    /// <summary>
    /// ゲーム終了時の演出（イベントから呼ばれる想定）
    /// </summary>
    public void ShowTimeUp()
    {
        introTextDisplay.text = "";
        outroTextDisplay.text = "";
        // メインテキストに赤文字で表示
        rubyInput.Text = "時間だ！";
    }
}