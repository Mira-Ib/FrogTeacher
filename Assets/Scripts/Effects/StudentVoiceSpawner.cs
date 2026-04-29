using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Imageコンポーネント用

public class StudentVoiceSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject voicePrefab; // 作成したPrefabを指定
    [SerializeField] private Transform spawnParent;   // 演出を表示するCanvas等の親

    [Header("Resources")]
    [SerializeField] private Sprite[] voiceSprites;   // 用意したPNG画像のSprite配列
    [SerializeField] private Color[] colorPalette;    // ランダム色の候補リスト

    [Header("Spawn Positions (座標指定)")]
    [SerializeField] private Vector2[] spawnPoints; // インスペクタで複数指定

    // 追加：シャッフルされたインデックスを保持するリスト
    private List<int> spriteIndices = new List<int>();
    private List<int> colorIndices = new List<int>();
    private List<int> pointIndices = new List<int>();

    private void OnEnable()
    {
        // QuizDirectorのイベントを購読
        QuizDirector.OnCorrect += SpawnCorrectVoice;
        QuizDirector.OnWrong += SpawnWrongVoice;
    }

    private void OnDisable()
    {
        // 解除を忘れない（メモリリーク防止）
        QuizDirector.OnCorrect -= SpawnCorrectVoice;
        QuizDirector.OnWrong -= SpawnWrongVoice;
    }

    // 正解時の演出トリガー
    private void SpawnCorrectVoice(QuizResultData data)
    {
        // 正解用の画像群があればそれを渡す、なければ共通、などの設計が可能
        Spawn(voiceSprites);
    }

    // 不正解時の演出トリガー
    private void SpawnWrongVoice()
    {
        Spawn(voiceSprites); // 今回は共通画像とする例
    }

    // 共通の生成ロジック
    private void Spawn(Sprite[] spriteCanditates)
    {
        if (spriteCanditates == null || spriteCanditates.Length == 0) return;
        if (voicePrefab == null) return;

        // 1. Prefabを生成
        GameObject go = Instantiate(voicePrefab, spawnParent);

        // 2. コンポーネントの取得
        Image image = go.GetComponent<Image>();
        StudentVoiceEffect effect = go.GetComponent<StudentVoiceEffect>(); // 後述のスクリプト

        if (image == null || effect == null)
        {
            Debug.LogError("PrefabにImageかStudentVoiceEffectが付いていません");
            Destroy(go);
            return;
        }

        // 3. 画像の決定（全巡重複なし）
        int sIdx = GetNextUniqueIndex(spriteIndices, spriteCanditates.Length);
        image.sprite = spriteCanditates[sIdx];
        image.SetNativeSize();

        // 4. 画像の大きさをPNG本来のサイズに合わせる
        image.SetNativeSize();

        // 5. 色の決定（全巡重複なし）
        if (colorPalette != null && colorPalette.Length > 0)
        {
            int cIdx = GetNextUniqueIndex(colorIndices, colorPalette.Length);
            image.color = colorPalette[cIdx];
        }

        // 6. 座標の決定（全巡重複なし）
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int pIdx = GetNextUniqueIndex(pointIndices, spawnPoints.Length);
            image.rectTransform.anchoredPosition = spawnPoints[pIdx];
        }

        // 7. アニメーション開始（自身に任せる）
        effect.StartEffect();
    }
    // 新しく追加するメソッド
    private int GetNextUniqueIndex(List<int> currentList, int totalCount)
    {
        // リストが空（または一巡した）なら、新しくインデックスを詰めてシャッフル
        if (currentList.Count == 0)
        {
            for (int i = 0; i < totalCount; i++) currentList.Add(i);

            // シャッフル（フィッシャー・イェーツの手法）
            for (int i = 0; i < currentList.Count; i++)
            {
                int temp = currentList[i];
                int randomIndex = Random.Range(i, currentList.Count);
                currentList[i] = currentList[randomIndex];
                currentList[randomIndex] = temp;
            }
        }

        // リストの先頭から1つ取り出して、リストから消す
        int index = currentList[0];
        currentList.RemoveAt(0);
        return index;
    }
}