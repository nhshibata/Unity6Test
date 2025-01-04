using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        var validRooms = GetRoomsWithFloor();

        int placedCount = 0;
        int targetCount = Math.Max(minCount, Math.Min(count, validRooms.Count));

        for (int i = 0; i < targetCount; i++)
        {
            if (validRooms.Count == 0)
                break;

            var roomIndex = random.Next(validRooms.Count);
            var room = validRooms[roomIndex];

            // 部屋内のランダム位置を取得
            var position = room[random.Next(room.Count)];
            map[position.y, position.x] = (char)type;
            room.Remove(position);

            if (room.Count == 0)
                validRooms.RemoveAt(roomIndex);

            placedCount++;
        }

        // 残り数をランダムな部屋に配置
        while (placedCount < count)
        {
            if (validRooms.Count == 0)
                validRooms = GetRoomsWithFloor();

            var roomIndex = random.Next(validRooms.Count);
            var room = validRooms[roomIndex];
            var position = room[random.Next(room.Count)];
            map[position.y, position.x] = (char)type;
            room.Remove(position);

            if (room.Count == 0)
                validRooms.RemoveAt(roomIndex);

            placedCount++;
        }
    }

    private List<List<(int x, int y)>> GetRoomsWithFloor()
    {
        var rooms = new List<List<(int x, int y)>>();

        var visited = new bool[mapHeight, mapWidth];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[y, x] == '.' && !visited[y, x])
                {
                    var room = new List<(int x, int y)>();
                    ExploreRoom(x, y, visited, room);
                    if (room.Count > 0)
                        rooms.Add(room);
                }
            }
        }

        return rooms;
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
