using UnityEngine;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private int firstVariable;
    [SerializeField] private int secondVariable;
    [SerializeField] private ResultView r;

    private void Start()
    {
        button.onClick.AddListener(() => r.ShowResult(firstVariable, secondVariable));
    }
}
