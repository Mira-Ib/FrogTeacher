using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NextScreenPanel))]
public class StageSelectManager : MonoBehaviour
{
    [Header("UIの参照")]
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("ボタンの参照")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [Tooltip("ステージ名が表示されている、決定を兼ねたボタン")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button backButton;

    [Header("システムの参照")]
    [SerializeField] private TitleTransitionCoordinator titleCoordinator;
    private NextScreenPanel _myPanel;

    [Header("ステージデータ")]
    [Tooltip("ここに作成したStageDataを順番に登録してください")]
    [SerializeField] private List<StageData> stageDataList;

    private StageSelectController _controller;

    private void Awake()
    {
        _myPanel = GetComponent<NextScreenPanel>();
    }

    private void Start()
    {
        // Controllerを生成して全てを委譲する
        _controller = new StageSelectController(
            stageNameText,
            descriptionText,
            leftButton,
            rightButton,
            submitButton,
            backButton,
            stageDataList,
            titleCoordinator,
            _myPanel,
            this.GetCancellationTokenOnDestroy()
        );
    }

    private void OnDestroy()
    {
        _controller?.Dispose();
    }
}