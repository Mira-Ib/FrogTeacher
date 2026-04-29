using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 責務: 全問題データの保持と、条件に応じたリスト（プール）の作成
public class QuestionLibrary : MonoBehaviour
{
    [SerializeField] private List<QuestionData> allMasterQuestions; // インスペクターで100問入れる

    // 1周目の仕分け用プール
    private QuestionData firstQuestion;
    private List<QuestionData> normalPool = new();
    private List<QuestionData> latePool = new();

    // 2周目以降用の全リスト
    private List<QuestionData> masterPool = new();

    private void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        masterPool = new List<QuestionData>(allMasterQuestions);

        foreach (var q in allMasterQuestions)
        {
            if (q.isFirstFixed) firstQuestion = q;
            else if (q.isLateGameOnly) latePool.Add(q);
            else normalPool.Add(q);
        }

        // エラーチェック
        if (firstQuestion == null) Debug.LogError("1問目固定の問題が設定されていません");
    }

    // 責務: 1周目用の、条件に沿ってシャッフルされたリストを作成して返す
    public List<QuestionData> GenerateFirstLoopSequence()
    {
        List<QuestionData> sequence = new List<QuestionData>();

        // 1. 固定の1問目
        sequence.Add(firstQuestion);

        // 2. 前半戦（残りのNormal問題をシャッフル）
        var shuffledNormal = normalPool.OrderBy(a => System.Guid.NewGuid()).ToList();

        // 50問目まではNormalから出す (1問目 + Normal49問)
        sequence.AddRange(shuffledNormal.Take(49));

        // 3. 後半戦（残りのNormal + Late問題をシャッフル）
        var remainingNormal = shuffledNormal.Skip(49);
        var lateGameMixedPool = remainingNormal.Concat(latePool).OrderBy(a => System.Guid.NewGuid()).ToList();

        sequence.AddRange(lateGameMixedPool);

        return sequence;
    }

    // 責務: 2周目以降用の、完全ランダムなリストを作成して返す
    public List<QuestionData> GenerateRandomSequence()
    {
        return masterPool.OrderBy(a => System.Guid.NewGuid()).ToList();
    }
}
