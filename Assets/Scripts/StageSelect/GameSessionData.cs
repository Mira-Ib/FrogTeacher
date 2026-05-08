/// <summary>
/// シーンを跨いでデータを保持するための静的クラス（共有スペース）
/// MonoBehaviourを継承しないため、GameObjectにアタッチする必要はありません。
/// </summary>
public static class GameSessionData
{
    // 選択されたステージデータを一時的に保存しておく場所
    public static StageData SelectedStage { get; set; }
}