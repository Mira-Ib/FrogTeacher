using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewQuizData", menuName = "Game/Quiz Data")]
public class QuizData : ScriptableObject
{
    [Header("このステージで出題される全問題")]
    public List<QuestionData> questions = new List<QuestionData>();

#if UNITY_EDITOR
    [Header("エディタ自動取得ツール")]
    [Tooltip("読み込みたいフォルダのパス（例: Assets/Resources/Questions/Stage1）")]
    public string targetFolderPath = "Assets/Resources/Questions/Stage1";

    // ★インスペクタの歯車マーク（または右クリック）から実行できる便利なコマンド
    [ContextMenu("このフォルダから問題を全自動ロードする")]
    public void LoadQuestionsFromFolder()
    {
        questions.Clear();

        // 指定フォルダ内にある「QuestionData」型のファイルを全て検索
        string[] guids = AssetDatabase.FindAssets("t:QuestionData", new[] { targetFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestionData q = AssetDatabase.LoadAssetAtPath<QuestionData>(path);
            if (q != null) questions.Add(q);
        }

        // 変更を保存
        EditorUtility.SetDirty(this);
        Debug.Log($"<color=green>【取得完了】</color> {targetFolderPath} から {questions.Count} 問のデータを読み込みました！");
    }
#endif
}