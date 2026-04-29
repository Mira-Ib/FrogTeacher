using DG.Tweening;
using UnityEngine;

public class ScoreEffectSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject scoreEffectPrefab;
    [SerializeField] private Transform spawnParent;

    [Header("Fixed Positions")]
    [SerializeField] private RectTransform comboPoint;     // 「〇 COMBO」が出る位置
    [SerializeField] private RectTransform multiPoint;     // 「x倍率」が出る位置
    [SerializeField] private RectTransform totalPoint;     // 「+合計点」が出る位置
    [SerializeField] private RectTransform wrongPoint;     // 不正解時の位置

    [Header("Settings")]
    public float appearDuration = 0.2f;
    public float totalDelay = 0.2f;      // コンボが出てから合計点が出るまでの時間差
    public float displayDuration = 0.8f;
    public float slideDuration = 0.4f;
    public float slideDistance = 500f;   // 右にどれくらいスライドするか

    [Header("Easings")]
    public Ease appearEase = Ease.OutBack;
    public Ease slideEase = Ease.InCubic;

    private void OnEnable()
    {
        QuizDirector.OnCorrect += SpawnCorrect;
        QuizDirector.OnWrong += SpawnWrong;
    }

    private void OnDisable()
    {
        QuizDirector.OnCorrect -= SpawnCorrect;
        QuizDirector.OnWrong -= SpawnWrong;
    }

    private void SpawnCorrect(QuizResultData data)
    {
        var item = Instantiate(scoreEffectPrefab, spawnParent).GetComponent<ScoreEffectItem>();
        item.PlayCorrect(data, comboPoint.position, multiPoint.position, totalPoint.position, this);
    }

    private void SpawnWrong()
    {
        var item = Instantiate(scoreEffectPrefab, spawnParent).GetComponent<ScoreEffectItem>();
        item.PlayWrong(wrongPoint.position, this);
    }
}