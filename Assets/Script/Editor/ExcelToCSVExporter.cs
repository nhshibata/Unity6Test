using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelToCSVExporter : EditorWindow
{
    [MenuItem("Tools/Export Excel to CSV")]
    public static void ShowWindow()
    {
        // エディタウィンドウを表示
        GetWindow<ExcelToCSVExporter>("Excel to CSV Exporter");
    }

    private string folderPath = "Assets/MasterData/ExcelFiles";
    private string outputFolder = "Assets/MasterData/CSVOutput";

    private void OnGUI()
    {
        GUILayout.Label("Excel to CSV Exporter", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Excel Files Folder", folderPath);
        outputFolder = EditorGUILayout.TextField("CSV Output Folder", outputFolder);

        if (GUILayout.Button("Export"))
        {
            ExportExcelToCSV();
        }
    }

    private void ExportExcelToCSV()
    {
        // フォルダ内のxlsxファイルを取得
        string[] excelFiles = Directory.GetFiles(folderPath, "*.xlsx");

        foreach (string filePath in excelFiles)
        {
            try
            {
                // Excelファイルを開く
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // シートを読み込む
                    do
                    {
                        // 各シートの名前を確認
                        string sheetName = reader.Name;

                        // シート名がコメントアウトされていない場合のみ処理
                        if (!IsCommentedSheetName(sheetName))
                        {
                            List<string[]> rows = new List<string[]>();

                            // 各行を読み込む
                            while (reader.Read())
                            {
                                List<string> row = new List<string>();

                                // 各セルの値を取得
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetValue(i)?.ToString() ?? string.Empty);
                                }

                                rows.Add(row.ToArray());
                            }

                            // CSVファイルに保存
                            string outputFilePath = Path.Combine(outputFolder, sheetName + ".csv");
                            SaveRowsAsCSV(rows, outputFilePath);
                        }
                    } while (reader.NextResult()); // 次のシートを読み込む
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read file {filePath}: {ex.Message}");
            }
        }

        AssetDatabase.Refresh(); // エディタのファイルリストを更新
        Debug.Log("Export completed!");
    }

    private bool IsCommentedSheetName(string sheetName)
    {
        // シート名がコメントアウトされている場合、Trueを返す
        return sheetName.StartsWith("#") || sheetName.StartsWith("//");
    }

    private void SaveRowsAsCSV(List<string[]> rows, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        // 各行をCSV形式で書き込む
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row));
        }

        // CSVとして保存
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        Debug.Log($"Exported {filePath}");
    }
}
