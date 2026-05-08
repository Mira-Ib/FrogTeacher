using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// アタッチしたUIオブジェクトから操作音（SE）を鳴らす専用スクリプト
/// </summary>
public class UIInteractionSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    [Header("カーソル移動（選択）時の音")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private SE hoverSound = SE.Transition;

    [Header("決定（クリック）時の音")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private SE clickSound = SE.Click;

    // 音の2重再生を防ぐためのタイマー
    private float _lastHoverSoundTime = -1f;

    // ==========================================
    // カーソルが合った時（Transition）の処理
    // ==========================================

    // 1. マウスが乗った時
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    // 2. キーボードやコントローラーで選択された時
    public void OnSelect(BaseEventData eventData)
    {
        PlayHoverSound();
    }

    private void PlayHoverSound()
    {
        if (!playHoverSound) return;

        // マウスが乗った瞬間に「PointerEnter」と「Select」が同時に呼ばれることがあるため、
        // 0.05秒以内の連続再生をブロックして2重に鳴るのを防ぎます
        if (Time.unscaledTime - _lastHoverSoundTime < 0.05f) return;

        _lastHoverSoundTime = Time.unscaledTime;
        PlaySoundCore(hoverSound);
    }

    // ==========================================
    // 決定した時（Click）の処理
    // ==========================================

    // 1. マウスでクリックされた時
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    // 2. キーボード（Enter）やコントローラーで決定された時
    public void OnSubmit(BaseEventData eventData)
    {
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (!playClickSound) return;
        PlaySoundCore(clickSound);
    }

    // ==========================================
    // 音を鳴らす共通処理
    // ==========================================
    private void PlaySoundCore(SE se)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(se);
        }
        else
        {
            Debug.LogWarning("AudioManagerが見つからないため、SEを再生できませんでした。");
        }
    }
}