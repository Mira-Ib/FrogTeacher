using UnityEngine;

public class QuizSE : MonoBehaviour
{
    private void OnEnable()
    {
        QuizDirector.OnCorrect += PlayCorrectSE;
        QuizDirector.OnWrong += PlayWrongSE;
    }

    private void OnDisable()
    {
        QuizDirector.OnCorrect -= PlayCorrectSE;
        QuizDirector.OnWrong -= PlayWrongSE;
    }

    private void PlayCorrectSE(QuizResultData data)
    {
        int comboCount = data.comboCount;
        if (comboCount == 1) AudioManager.Instance.PlaySE(SE.Correct_1);
        else if (comboCount == 2) AudioManager.Instance.PlaySE(SE.Correct_2);
        else if (comboCount == 3) AudioManager.Instance.PlaySE(SE.Correct_3);
        else if (comboCount == 4) AudioManager.Instance.PlaySE(SE.Correct_4);
        else if (comboCount == 5) AudioManager.Instance.PlaySE(SE.Correct_5);
        else if (comboCount == 6) AudioManager.Instance.PlaySE(SE.Correct_6);
        else if (comboCount == 7) AudioManager.Instance.PlaySE(SE.Correct_7);
        else AudioManager.Instance.PlaySE(SE.Correct_8);
    }

    private void PlayWrongSE()
    {
        AudioManager.Instance.PlaySE(SE.Wrong);
    }
}
