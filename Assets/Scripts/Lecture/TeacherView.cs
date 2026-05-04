using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TeacherView : MonoBehaviour, ILecturePlayable
{
    [SerializeField] private RectTransform teacherRect;
    [SerializeField] private Image teacherImage;

    [Header("カエル先生の画像設定（授業用）")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite handLoweredSprite;

    [Header("登場アニメーション設定")]
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private float enterDuration = 1.0f;

    [Header("クイズ用スクリプトとの連携")]
    [Tooltip("ここにFrogTeacherスクリプトをアタッチしてください")]
    [SerializeField] private FrogTeacher frogTeacherScript;

    private Tween _enterTween;

    public async UniTask EnterTeacherAsync(CancellationToken token)
    {
        // 授業パート開始時：クイズ用の自動アニメーションを「オフ」にする
        if (frogTeacherScript != null)
        {
            frogTeacherScript.enabled = false;
        }

        teacherImage.sprite = normalSprite;
        teacherRect.anchoredPosition = startPosition;

        _enterTween = teacherRect.DOAnchorPos(endPosition, enterDuration)
                                 .SetEase(Ease.Linear);

        await _enterTween.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken: token);
    }

    public void SetHandState(bool isHandLowered)
    {
        // 授業中はここで画像が切り替わる（FrogTeacherがオフなので上書きされない）
        teacherImage.sprite = isHandLowered ? handLoweredSprite : normalSprite;
    }

    public void FastForward()
    {
        _enterTween?.Kill();
        teacherRect.anchoredPosition = endPosition;
    }

    /// <summary>
    /// 授業が完了し、クイズパートへ移行する際にManagerから呼ばれる
    /// </summary>
    public void SwitchToQuizMode()
    {
        // クイズパート開始時：自動アニメーションを「オン」に戻す
        if (frogTeacherScript != null)
        {
            frogTeacherScript.enabled = true;
        }
    }
}