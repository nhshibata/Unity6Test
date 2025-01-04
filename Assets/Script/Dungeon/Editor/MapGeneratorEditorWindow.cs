#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

public class MapGeneratorEditorWindow : EditorWindow
{
    private string csvFilePath;
    private MapTileConfig selectedTileConfig;

    [MenuItem("Tools/Map Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<MapGeneratorEditorWindow>("Map Generator");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Generator", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // CSV ファイル選択
        if (GUILayout.Button("Select CSV File"))
        {
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        }

        if (!string.IsNullOrEmpty(csvFilePath))
        {
            GUILayout.Label($"Selected File: {Path.GetFileName(csvFilePath)}");
        }

        EditorGUILayout.Space();

        // MapTileConfig 選択
        selectedTileConfig = (MapTileConfig)EditorGUILayout.ObjectField("Tile Config", selectedTileConfig, typeof(MapTileConfig), false);

        EditorGUILayout.Space();

        // マップ生成ボタン
        if (GUILayout.Button("Generate Map"))
        {
            if (selectedTileConfig == null || string.IsNullOrEmpty(csvFilePath))
            {
                Debug.LogError("CSVファイルがありません");
            }
            else
            {
                GenerateMap();
            }
        }
    }

    private void GenerateMap()
    {
        string[] lines = File.ReadAllLines(csvFilePath);

        GameObject mapParent = new GameObject("GeneratedMap");

        for (int y = 0; y < lines.Length; y++)
        {
            // 行を読み込み、コンマを取り除く
            string line = lines[y].Replace(",", "");

            for (int x = 0; x < line.Length; x++)
            {
                char tile = line[x];

                GameObject prefab = selectedTileConfig.GetGameObjectForTile(tile);

                if (prefab != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.transform.position = new Vector3(x, 0, -y);
                    instance.transform.SetParent(mapParent.transform);
                }
                else
                {
                    Debug.LogWarning($"{tile}のprefabが設定されていません");
                }
            }
        }

        Debug.Log("マップ生成成功");
    }
}

#endif
