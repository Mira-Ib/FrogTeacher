using UnityEngine;
using UnityEngine.UI;

public class StageSelectByKey : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button backButton;


    private void Update()
    {
        // 左キー または Aキー → 左ボタン (SE: Transition)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PressButton(leftButton, SE.Transition);
        }
        // 右キー または Dキー → 右ボタン (SE: Transition)
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            PressButton(rightButton, SE.Transition);
        }
        // Enterキー（メイン / テンキー） → 決定ボタン (音なし)
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PressButton(submitButton);
        }
        // Backspaceキー → 戻るボタン (SE: Back)
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            PressButton(backButton, SE.Back);
        }
    }

    /// <summary>
    /// ボタンの処理のみを実行する（音なし）
    /// </summary>
    private void PressButton(Button button)
    {
        if (button == null) return;
        button.onClick.Invoke();
    }

    /// <summary>
    /// ボタンの処理を実行し、指定したSEを再生する
    /// </summary>
    private void PressButton(Button button, SE se)
    {
        PressButton(button);
        PlaySound(se);
    }

    private void PlaySound(SE se)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(se);
        }
    }
}