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

        // GameObjectをマップの親オブジェクトとして作成
        GameObject mapParent = new GameObject("GeneratedMap");

        // マップのサイズ（行数と列数）を計算
        int mapWidth = lines[0].Length;
        int mapHeight = lines.Length;

        // マップの中心を (0, 0, 0) に合わせるためのオフセット
        float offsetX = mapWidth / 2f;
        float offsetZ = mapHeight / 2f;

        for (int y = 0; y < lines.Length; y++)
        {
            // 行を読み込み、コンマを取り除く
            string line = lines[y].Replace(",", "");

            for (int x = 0; x < line.Length; x++)
            {
                char tile = line[x];

                // タイルに対応するPrefabを取得
                GameObject prefab = selectedTileConfig.GetGameObjectForTile(tile);

                if (prefab != null)
                {
                    // Prefabをインスタンス化
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                    // 座標を設定 (x, 0, -z)にすることで、マップ中央が (0, 0, 0) になる
                    instance.transform.position = new Vector3(x - offsetX, 0, -(y - offsetZ));

                    // 親オブジェクトを設定
                    instance.transform.SetParent(mapParent.transform);
                }
                else
                {
                    // Prefabが設定されていない場合は警告
                    Debug.LogWarning($"{tile}のprefabが設定されていません");
                }
            }
        }

        // マップ生成成功のログ
        Debug.Log("マップ生成成功");
    }

}

#endif
