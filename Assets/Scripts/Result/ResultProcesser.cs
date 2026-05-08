using UnityEngine;

public struct ResultDisplayData
{
    public int GettingScore;
    public int PenaltyCount;
    public int TotalScore;
    public string Rank;
    public string TeacherComment;
}

public static class ResultProcessor
{
    private static readonly string[] Ranks = { "C", "B", "B+", "B++", "A", "A+", "A++", "S", "S+", "S++" };

    public static ResultDisplayData Process(int gettingScore, int penaltyCount)
    {
        // 合計点の計算（マイナスは0に丸める）
        int totalScore = Mathf.Max(0, gettingScore - (penaltyCount * 30));

        // ランク判定（10段階の例：スコアに応じてインデックスを決定）
        int rankIndex = 0;
        if (totalScore < 300) { rankIndex = 0; }
        else if (totalScore < 500) { rankIndex = 1; }
        else if (totalScore < 700) { rankIndex = 2; }
        else if (totalScore < 900) { rankIndex = 3; }
        else if (totalScore < 1100) { rankIndex = 4; }
        else if (totalScore < 1300) { rankIndex = 5; }
        else if (totalScore < 1500) { rankIndex = 6; }
        else if (totalScore < 2000) { rankIndex = 7; }
        else if (totalScore < 2500) { rankIndex = 8; }
        else { rankIndex = 9; }

        string rank = Ranks[rankIndex];

        return new ResultDisplayData
        {
            GettingScore = gettingScore,
            PenaltyCount = penaltyCount,
            TotalScore = totalScore,
            Rank = rank,
            TeacherComment = GetTeacherComment(rank)
        };
    }

    private static string GetTeacherComment(string rank)
    {
        return rank switch
        {
            "S++" => "ノーベル賞級の理解ケロ！",
            "S+" => "超天才級の理解ケロ！",
            "S" => "天才級の理解ケロ！",
            "A+" or "A++" => "結構分かってくれたみたいケロ！",
            "A" or "B++" => "そこそこ分かってくれたみたいケロ。",
            "B" or "B+" => "まぁまぁ分かってくれたみたいケロ。",
            _ => "授業って難しいケロ…",
        };
    }
}