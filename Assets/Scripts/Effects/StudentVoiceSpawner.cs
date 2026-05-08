using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentVoiceSpawner : MonoBehaviour
{
    [Header("Systems")]
    // ★追加：QuizDirectorを参照して、現在の問題データを読み取る
    [SerializeField] private QuizDirector quizDirector;

    [Header("Prefab Settings")]
    [SerializeField] private GameObject voicePrefab;
    [SerializeField] private Transform spawnParent;

    [Header("Resources (4パターンの画像群)")]
    [SerializeField] private Sprite[] correctTrueSprites;  // 正解 ＋ そうケロ
    [SerializeField] private Sprite[] correctFalseSprites; // 正解 ＋ ちがうケロ
    [SerializeField] private Sprite[] wrongTrueSprites;    // 不正解 ＋ そうケロ
    [SerializeField] private Sprite[] wrongFalseSprites;   // 不正解 ＋ ちがうケロ

    [SerializeField] private Color[] colorPalette;
    [Header("Spawn Positions (座標指定)")]
    [SerializeField] private Vector2[] spawnPoints;

    // ★修正：配列の枚数が違う場合のエラーを防ぐため、インデックス管理も4つに分けます
    private List<int> idxCorrectTrue = new List<int>();
    private List<int> idxCorrectFalse = new List<int>();
    private List<int> idxWrongTrue = new List<int>();
    private List<int> idxWrongFalse = new List<int>();

    private List<int> colorIndices = new List<int>();
    private List<int> pointIndices = new List<int>();

    private void OnEnable()
    {
        QuizDirector.OnCorrect += SpawnCorrectVoice;
        QuizDirector.OnWrong += SpawnWrongVoice;
    }

    private void OnDisable()
    {
        QuizDirector.OnCorrect -= SpawnCorrectVoice;
        QuizDirector.OnWrong -= SpawnWrongVoice;
    }

    // 正解時の演出トリガー
    private void SpawnCorrectVoice(QuizResultData data)
    {
        if (quizDirector == null || quizDirector.CurrentQuestionData == null) return;

        // 問題の「そうケロ/ちがうケロ」判定を見て配列を渡す
        if (quizDirector.CurrentQuestionData.isTrueKero)
        {
            Spawn(correctTrueSprites, idxCorrectTrue);
        }
        else
        {
            Spawn(correctFalseSprites, idxCorrectFalse);
        }
    }

    // 不正解時の演出トリガー
    private void SpawnWrongVoice()
    {
        if (quizDirector == null || quizDirector.CurrentQuestionData == null) return;

        // 不正解時も同様に判定
        if (quizDirector.CurrentQuestionData.isTrueKero)
        {
            Spawn(wrongTrueSprites, idxWrongTrue);
        }
        else
        {
            Spawn(wrongFalseSprites, idxWrongFalse);
        }
    }

    // ★修正：どのインデックスリストを使うかを引数で受け取る
    private void Spawn(Sprite[] spriteCandidates, List<int> specificIndices)
    {
        if (spriteCandidates == null || spriteCandidates.Length == 0) return;
        if (voicePrefab == null) return;

        GameObject go = Instantiate(voicePrefab, spawnParent);

        Image image = go.GetComponent<Image>();
        StudentVoiceEffect effect = go.GetComponent<StudentVoiceEffect>();

        if (image == null || effect == null)
        {
            Debug.LogError("PrefabにImageかStudentVoiceEffectが付いていません");
            Destroy(go);
            return;
        }

        // 3. 画像の決定（専用のインデックスリストを使う）
        int sIdx = GetNextUniqueIndex(specificIndices, spriteCandidates.Length);
        image.sprite = spriteCandidates[sIdx];

        // 4. 画像の大きさをPNG本来のサイズに合わせる
        image.SetNativeSize();

        // 5. 色の決定
        if (colorPalette != null && colorPalette.Length > 0)
        {
            int cIdx = GetNextUniqueIndex(colorIndices, colorPalette.Length);
            image.color = colorPalette[cIdx];
        }

        // 6. 座標の決定
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int pIdx = GetNextUniqueIndex(pointIndices, spawnPoints.Length);
            image.rectTransform.anchoredPosition = spawnPoints[pIdx];
        }

        // 7. アニメーション開始
        effect.StartEffect();
    }

    private int GetNextUniqueIndex(List<int> currentList, int totalCount)
    {
        if (currentList.Count == 0)
        {
            for (int i = 0; i < totalCount; i++) currentList.Add(i);

            for (int i = 0; i < currentList.Count; i++)
            {
                int temp = currentList[i];
                int randomIndex = Random.Range(i, currentList.Count);
                currentList[i] = currentList[randomIndex];
                currentList[randomIndex] = temp;
            }
        }

        int index = currentList[0];
        currentList.RemoveAt(0);
        return index;
    }
}