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

    // ★追加：暗転（フェードアウト）用のCanvasGroup
    [Tooltip("暗転演出に使用する、画面全体を覆う黒いImageのCanvasGroup")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

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
        // 引数の順番を StageSelectController のコンストラクタに合わせました
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
            blackoutCanvasGroup, // ★追加：CanvasGroupを渡す
            this.GetCancellationTokenOnDestroy()
        );
    }

    private void OnDestroy()
    {
        _controller?.Dispose();
    }
}