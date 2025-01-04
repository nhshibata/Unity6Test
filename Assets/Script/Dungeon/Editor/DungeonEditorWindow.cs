#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class DungeonEditorWindow : EditorWindow
{
    private int mapWidth = 200;
    private int mapHeight = 200;
    private int minRoomSize = 6;
    private int maxRoomSize = 12;
    private int minRooms = 2;
    private int maxRooms = 10;
    private string fileName = "dungeon.csv";

    private DungeonGenerator dungeonGenerator;
    private DungeonDataPopulator dataPopulator;
    private MaptipListBase maptipsList;

    [MenuItem("Tools/Dungeon Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<DungeonEditorWindow>("Dungeon Generator");
        window.minSize = new Vector2(300, 400);
    }

    private void OnGUI()
    {
        GUILayout.Label("1-1 Dungeon Generator Settings", EditorStyles.boldLabel);

        mapWidth = EditorGUILayout.IntField("Map Width", mapWidth);
        mapHeight = EditorGUILayout.IntField("Map Height", mapHeight);
        minRoomSize = EditorGUILayout.IntField("Min Room Size", minRoomSize);
        maxRoomSize = EditorGUILayout.IntField("Max Room Size", maxRoomSize);
        minRooms = EditorGUILayout.IntField("Min Rooms", minRooms);
        maxRooms = EditorGUILayout.IntField("Max Rooms", maxRooms);

        EditorGUILayout.Space();

        if (GUILayout.Button("1-2 Generate Dungeon"))
        {
            GenerateDungeon();
        }

        if (GUILayout.Button("1-3 Save Dungeon to CSV"))
        {
            SaveDungeonToCsv();
        }

        EditorGUILayout.Space();

        GUILayout.Label("2-1 Maptips Settings", EditorStyles.boldLabel);
        maptipsList = (MaptipListBase)EditorGUILayout.ObjectField("Maptip List", maptipsList, typeof(MaptipListBase), false);

        if (GUILayout.Button("2-2 Select CSV File"))
        {
            fileName = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("2-3 Populate Dungeon Data"))
        {
            PopulateDungeonData();
        }
    }

    private void GenerateDungeon()
    {
        dungeonGenerator = new DungeonGenerator();
        dungeonGenerator.GenerateDungeon(mapWidth, mapHeight, minRoomSize, maxRoomSize, minRooms, maxRooms);
        dataPopulator = new DungeonDataPopulator();
        if (!string.IsNullOrEmpty(fileName))
        {
            dataPopulator.LoadMapFromCsv(fileName);
        }
        Debug.Log("ダンジョン生成成功");
    }

    private void SaveDungeonToCsv()
    {
        if (dungeonGenerator == null)
        {
            Debug.LogError("ダンジョンが生成されていません");
            return;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("ダンジョンファイルがありません");
            return;
        }

        dungeonGenerator.SaveToCsv(fileName);
        Debug.Log($"Dungeon saved to CSV: {fileName}");
    }

    private void PopulateDungeonData()
    {
        dataPopulator = new DungeonDataPopulator();
        if (!string.IsNullOrEmpty(fileName))
        {
            dataPopulator.LoadMapFromCsv(fileName);
        }
        else
        {
            return;
        }

        if (maptipsList == null)
        {
            Debug.LogError("マップ設定情報なし");
            return;
        }

        Debug.Log($"マップに設置:{maptipsList.GetList().Count}");
        foreach (var maptips in maptipsList.GetList())
        {
            if (maptips == null)
                continue;

            dataPopulator.PlaceItemsInRooms(maptips.GetType(), maptips.GetCount(), maptips.GetMinCount());
        }

        dataPopulator.SaveToCsv(fileName);
        Debug.Log("マップ情報更新完了");
    }
}

#endif
