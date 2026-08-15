using UnityEngine;
using TMPro;
using TMP_Ruby; // 以前解決した名前空間
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

// このスクリプトをアタッチすると、必要なコンポーネントも自動で追加されます
[RequireComponent(typeof(TextMeshProRuby))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private float charactersPerSecond = 20f;

    private TextMeshProRuby _rubyText;
    private TextMeshProUGUI _mainText;
    private Tween _typewriterTween;

    private void Awake()
    {
        // GetComponentで同じオブジェクトについているコンポーネントを取得
        _rubyText = GetComponent<TextMeshProRuby>();
        _mainText = GetComponent<TextMeshProUGUI>();
    }

    public async UniTask PlayTypewriterAsync(string content, CancellationToken token)
    {
        // 1. 以前のスクリプト同様、大文字の Text プロパティに文字列を流し込む
        _rubyText.Text = content;

        // 2. 【超重要】タグを除外した文字数を正確に取得するため、一度メッシュを強制更新する
        _mainText.ForceMeshUpdate();

        // 3. タグ（<ruby>や<color>など）を除外した「純粋な表示文字数」を取得
        int totalVisibleChars = _mainText.textInfo.characterCount;

        // 4. 初期状態：文字をすべて隠す
        _mainText.maxVisibleCharacters = 0;

        // 5. アニメーション時間の計算（純粋な文字数を使うことでテンポが崩れない）
        float duration = totalVisibleChars / charactersPerSecond;

        // 6. DOTweenで文字送り
        _typewriterTween = DOTween.To(
            () => _mainText.maxVisibleCharacters,
            x => _mainText.maxVisibleCharacters = x,
            totalVisibleChars,
            duration
        ).SetEase(Ease.Linear);

        await _typewriterTween.ToUniTask(TweenCancelBehaviour.CancelAwait, cancellationToken: token);
    }

    public void FastForward()
    {
        _typewriterTween?.Kill();
        if (_typewriterTween != null && _typewriterTween.IsActive())
        {
            _typewriterTween.Kill();
        }

        // スキップ時は「純粋な表示文字数」を代入して全表示する
        if (_mainText != null && _mainText.textInfo != null)
        {
            _mainText.maxVisibleCharacters = _mainText.textInfo.characterCount;
        }
    }
    /// <summary>
    /// 会話中のクリック入力時に、現在の1文だけを最後まで即座に表示する（専用追加）
    /// </summary>
    public void CompleteTypewriter()
    {
        if (_typewriterTween != null && _typewriterTween.IsActive())
        {
            _typewriterTween.Kill();
        }

        if (_mainText != null && _mainText.textInfo != null)
        {
            _mainText.maxVisibleCharacters = _mainText.textInfo.characterCount;
        }
    }
}