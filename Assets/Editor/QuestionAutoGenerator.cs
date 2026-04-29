/*using UnityEngine;
using UnityEditor;
using System.IO;

public class QuestionAutoGenerator : EditorWindow
{
    private string savePath = "Assets/Questions"; // 保存先フォルダ
    private int count = 100; // 生成する数

    [MenuItem("Tools/Create 100 Questions")]
    public static void ShowWindow()
    {
        GetWindow<QuestionAutoGenerator>("Question Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("QuestionDataを一括生成します", EditorStyles.boldLabel);
        savePath = EditorGUILayout.TextField("保存パス", savePath);
        count = EditorGUILayout.IntField("生成数", count);

        if (GUILayout.Button("一括生成開始！"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        // フォルダがなければ作成
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        for (int i = 1; i <= count; i++)
        {
            // インスタンス作成
            QuestionData asset = ScriptableObject.CreateInstance<QuestionData>();

            // ファイル名を Q001, Q002... 形式にする
            string fileName = $"Q{i:D3}.asset";
            string fullPath = Path.Combine(savePath, fileName);

            // アセットとして保存
            AssetDatabase.CreateAsset(asset, fullPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{count}個のQuestionDataを生成しました！");
    }
} */