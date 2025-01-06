using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

public class NameConstantsGenerator : EditorWindow
{
    private Object targetAsset;
    private string outputPath = "Assets";

    [MenuItem("Tools/Name Constants Generator")]
    public static void ShowWindow()
    {
        GetWindow<NameConstantsGenerator>("Name Constants Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Constants from .asset file", EditorStyles.boldLabel);

        // ターゲットアセットの選択
        targetAsset = EditorGUILayout.ObjectField("Target Asset", targetAsset, typeof(Object), false);

        // フォルダ選択ボタン
        if (GUILayout.Button("Select Output Folder"))
        {
            if (targetAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(targetAsset);
                string folderPath = Path.GetDirectoryName(assetPath);

                // フォルダ選択ダイアログを開く
                string selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");

                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    outputPath = selectedFolder;
                    Debug.Log($"Output folder selected: {outputPath}");
                }
                else
                {
                    Debug.LogWarning("フォルダが選択されていません");
                }
            }
            else
            {
                Debug.LogError("asset ファイルが選択されていません");
            }
        }

        // 定数生成ボタン
        if (GUILayout.Button("Generate Constants"))
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                Debug.LogError("保存先が指定されていません");
                return;
            }

            GenerateConstants();
        }
    }


    private void GenerateConstants()
    {
        if (targetAsset == null)
        {
            Debug.LogError("アセットファイルが選択されていません");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(targetAsset);
        if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".asset"))
        {
            Debug.LogError("選択されたファイルは有効な .asset ファイルではありません");
            return;
        }

        // 出力クラス名の設定
        string outputClassName = Path.GetFileNameWithoutExtension(assetPath) + "BlackboardConstants";

        string fileContent = File.ReadAllText(assetPath);

        // m_Blackboard ～ --- ! の範囲を抽出
        Regex rangeRegex = new Regex(@"m_Blackboard:(.*?)(?=--- !)", RegexOptions.Singleline);
        Match rangeMatch = rangeRegex.Match(fileContent);

        if (!rangeMatch.Success)
        {
            Debug.LogWarning("アセットファイル内に有効な m_Blackboard セクションが見つかりませんでした。");
            return;
        }

        string blackboardSection = rangeMatch.Value;

        // Name: のみを抽出
        Regex nameRegex = new Regex(@"^\s*Name:\s*(.+)", RegexOptions.Multiline);
        MatchCollection nameMatches = nameRegex.Matches(blackboardSection);

        if (nameMatches.Count == 0)
        {
            Debug.LogWarning($"m_Blackboard セクション内に有効な 'Name:' フィールドが見つかりませんでした。内容: {rangeMatch.Value}");
            return;
        }

        HashSet<string> uniqueNames = new HashSet<string>();
        foreach (Match match in nameMatches)
        {
            string value = match.Groups[1].Value.Trim();
            uniqueNames.Add(value);
        }

        if (uniqueNames.Count == 0)
        {
            Debug.LogWarning("定数生成用の変数が見つかりません");
            return;
        }

        // 定数クラスを生成
        string classContent = GenerateClassContent(outputClassName, uniqueNames);

        // スクリプトの保存
        string fullPath = Path.Combine(outputPath, $"{outputClassName}.cs");
        File.WriteAllText(fullPath, classContent);
        AssetDatabase.Refresh();

        Debug.Log($"定数クラスを生成しました: {fullPath}");
    }

    private string GenerateClassContent(string className, HashSet<string> uniqueNames)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("public static class " + className);
        builder.AppendLine("{");

        foreach (string name in uniqueNames)
        {
            string constName = Regex.Replace(name, @"\s+|-|\.", "_").Replace("__", "_");
            builder.AppendLine($"    public const string {constName} = \"{name}\";");
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

}
