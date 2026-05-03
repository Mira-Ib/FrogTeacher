using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TeacherView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private RectTransform teacherRect;
    [SerializeField] private Image teacherImage;

    [Header("カエル先生の画像設定")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite handRaisedSprite;

    [Header("登場アニメーション設定")]
    [SerializeField] private Vector2 startPosition; // 画面外（右側）の座標
    [SerializeField] private Vector2 endPosition;   // 画面右下の停止座標
    [SerializeField] private float enterDuration = 1.0f;

    private Tween _enterTween;

    public async UniTask EnterTeacherAsync(CancellationToken token)
    {
        teacherImage.sprite = normalSprite;
        teacherRect.anchoredPosition = startPosition;

        // 登場アニメーション（ゆっくり減速しながら定位置へ）
        _enterTween = teacherRect.DOAnchorPos(endPosition, enterDuration)
                                 .SetEase(Ease.Linear);

        await _enterTween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
    }

    public void SetHandState(bool isHandRaised)
    {
        // 状態に応じてSpriteを差し替える（一瞬で切り替わる）
        teacherImage.sprite = isHandRaised ? handRaisedSprite : normalSprite;
    }

    public void FastForward()
    {
        // 登場アニメーション中にSキーが押されたら強制終了して所定の位置へ
        _enterTween?.Kill();
        teacherRect.anchoredPosition = endPosition;
    }
}