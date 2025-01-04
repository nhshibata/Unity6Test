using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static DungeonGenerator;

public class DungeonDataPopulator
{
    private char[,] map; // 元のマップ
    private int mapWidth; // マップの幅
    private int mapHeight; // マップの高さ
    private System.Random random; // ランダム生成器

    public DungeonDataPopulator()
    {
        random = new System.Random();
    }

    public void LoadMapFromCsv(string fileName)
    {
        string path = GetSavePath(fileName);
        var lines = File.ReadAllLines(path);
        mapHeight = lines.Length;
        mapWidth = lines[0].Split(',').Length;
        map = new char[mapHeight, mapWidth];

        for (int y = 0; y < mapHeight; y++)
        {
            var row = lines[y].Split(',');
            for (int x = 0; x < mapWidth; x++)
            {
                map[y, x] = row[x][0];
            }
        }
    }

    public void PlaceItemsInRooms(int type, int count, int minCount)
    {
        // 部屋ごとの床座標リストを取得
        var validRooms = GetRooms();

        int targetCount = Math.Max(minCount, Math.Min(count, validRooms.Sum(room => room.Count)));

        Debug.Log($"{type}を{targetCount}個配置");

        for (int i = 0; i < targetCount; i++)
        {
            if (validRooms.Count == 0)
                break;

            // ランダムな部屋を選択
            var roomIndex = random.Next(validRooms.Count);
            var room = validRooms[roomIndex];

            Debug.Log($"{roomIndex}番号{validRooms.Count}部屋");

            // ランダムな部屋内座標を選択
            var tileIndex = random.Next(room.Count);
            var position = room[tileIndex];

            // アイテムを配置
            map[position.y, position.x] = (char)('0' + type);

            // 配置したタイルを削除
            room.RemoveAt(tileIndex);

            // 部屋内の床タイルがなくなった場合、その部屋を削除
            if (room.Count == 0)
            {
                validRooms.RemoveAt(roomIndex);
            }
        }
    }

    private List<List<(int x, int y)>> GetRooms()
    {
        var rooms = new List<List<(int x, int y)>>();
        var visited = new HashSet<(int x, int y)>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // 外周タイル（RoomOuter）を検出
                if (DungeonBaseTip.IsMatch(map[y, x], DungeonBaseTip.Tip.RoomOuter) && !visited.Contains((x, y)))
                {
                    // 部屋を特定
                    var roomBounds = GetRoomBounds(x, y, visited);
                    var roomTiles = CollectInnerTiles(roomBounds);

                    if (roomTiles.Count > 0)
                    {
                        rooms.Add(roomTiles);
                    }
                }
            }
        }

        return rooms;
    }

    private RectInt GetRoomBounds(int startX, int startY, HashSet<(int x, int y)> visited)
    {
        int minX = startX, maxX = startX;
        int minY = startY, maxY = startY;
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));
        visited.Add((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            // 範囲を更新
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);

            // 隣接する外周タイルを探索
            var directions = new (int dx, int dy)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            foreach (var (dx, dy) in directions)
            {
                int newX = x + dx;
                int newY = y + dy;

                if (IsInsideMap(newX, newY) &&
                    DungeonBaseTip.IsMatch(map[newY, newX], DungeonBaseTip.Tip.RoomOuter) &&
                    !visited.Contains((newX, newY)))
                {
                    queue.Enqueue((newX, newY));
                    visited.Add((newX, newY));
                }
            }
        }

        // 長方形の範囲を返す
        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private List<(int x, int y)> CollectInnerTiles(RectInt roomBounds)
    {
        var innerTiles = new List<(int x, int y)>();

        for (int y = roomBounds.yMin + 1; y < roomBounds.yMax; y++)
        {
            for (int x = roomBounds.xMin + 1; x < roomBounds.xMax; x++)
            {
                if (DungeonBaseTip.IsMatch(map[y, x], DungeonBaseTip.Tip.RoomInner))
                {
                    innerTiles.Add((x, y));
                }
            }
        }

        return innerTiles;
    }



    private bool IsInsideMap(int x, int y)
    {
        return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
    }

    private void ExploreRoom(int x, int y, bool[,] visited, List<(int x, int y)> room)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight || visited[y, x] || map[y, x] != '.')
            return;

        visited[y, x] = true;
        room.Add((x, y));

        ExploreRoom(x + 1, y, visited, room);
        ExploreRoom(x - 1, y, visited, room);
        ExploreRoom(x, y + 1, visited, room);
        ExploreRoom(x, y - 1, visited, room);
    }

    public void SaveToCsv(string originalFileName)
    {
        string newFileName = GetNextFileName(originalFileName);
        string path = GetSavePath(newFileName);

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var writer = new StreamWriter(path))
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    writer.Write(map[y, x]);
                    if (x < mapWidth - 1)
                    {
                        writer.Write(",");
                    }
                }
                writer.WriteLine();
            }
        }
        Debug.Log($"Dungeon saved to CSV: {newFileName}");
    }

    private string GetSavePath(string fileName)
    {
        return Path.Combine(Application.dataPath, "CSV", fileName);
    }

    private string GetNextFileName(string originalFileName)
    {
        string baseName = Path.GetFileNameWithoutExtension(originalFileName);
        string directory = Path.GetDirectoryName(originalFileName);
        string extension = Path.GetExtension(originalFileName);

        if (baseName.Contains("_convert"))
        {
            int index = baseName.LastIndexOf("_convert");
            string prefix = baseName.Substring(0, index);
            string postfix = baseName.Substring(index + "_convert".Length);

            if (int.TryParse(postfix, out int n))
            {
                n++;
                return Path.Combine(directory, $"{prefix}_convert{n}{extension}");
            }
        }

        return Path.Combine(directory, $"{baseName}_convert1{extension}");
    }
}
