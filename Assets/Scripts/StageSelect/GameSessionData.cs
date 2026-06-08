/// <summary>
/// シーンを跨いでデータを保持するための静的クラス（共有スペース）
/// MonoBehaviourを継承しないため、GameObjectにアタッチする必要はありません。
/// </summary>
public static class GameSessionData
{
    // ★追加：ポーズ画面を一度でも開いたかどうかを記録するフラグ
    public static bool HasUsedPause { get; private set; }

    // ★追加：ゲーム（ステージ）開始時にフラグを綺麗にリセットするためのメソッド
    public static void ResetSessionStatus()
    {
        HasUsedPause = false;
        // 他にリセットすべきセッションデータがあればここに書きます
    }
    // ★追加：ポーズが使われたことを外部から通知する用
    public static void FlagAsPauseUsed()
    {
        HasUsedPause = true;
    }
    // 選択されたステージデータを一時的に保存しておく場所
    public static StageData SelectedStage { get; set; }
}