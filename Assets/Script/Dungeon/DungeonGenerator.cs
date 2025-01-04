using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DungeonGenerator
{
    public static class DungeonBaseTip
    {
        public enum Tip
        {
            Wall,    // 壁
            Passage, // 通路
            RoomOuter,  // 部屋の外周
            RoomInner   // 部屋の内部
        }

        public static char GetChar(Tip tip)
        {
            return tip switch
            {
                Tip.Wall => '#',         // 壁は '#'
                Tip.Passage => '.',      // 通路は '.'
                Tip.RoomOuter => '@',    // 部屋の外周は '@'
                Tip.RoomInner => '&',    // 部屋の内部は '&'
                _ => throw new NotImplementedException() // 未実装のケース
            };
        }

        public static bool IsMatch(char c, Tip tip)
        {
            return c == GetChar(tip);
        }
    }

    private int mapWidth = 200;   // マップの幅
    private int mapHeight = 200;  // マップの高さ
    private int minRoomSize = 36;  // 部屋の最小サイズ
    private int maxRoomSize = 144; // 部屋の最大サイズ
    private int minRooms = 3;     // 最大部屋数
    private int maxRooms = 10;    // 最大部屋数

    private char[,] map;

    public void GenerateDungeon(int mapWidth, int mapHeight, int minRoomSize, int maxRoomSize, int minRooms, int maxRooms)
    {
        // パラメータを反映
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.minRooms = minRooms;
        this.maxRooms = maxRooms;

        // 既存のロジックを呼び出す
        GenerateDungeon();
    }

    private void InitializeMap()
    {
        map = new char[mapHeight, mapWidth];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[y, x] = DungeonBaseTip.GetChar(DungeonBaseTip.Tip.Wall); // 壁で初期化
            }
        }
    }

    public void GenerateDungeon()
    {
        InitializeMap();
        var rooms = new List<Room>();
        var random = new System.Random();
        int roomNum = random.Next(minRooms, maxRooms);

        // 部屋を生成
        for (int i = 0; i < roomNum; i++)
        {
            // 部屋の面積をランダムに設定（最小・最大面積に収める）
            int roomArea = random.Next(minRoomSize, maxRoomSize + 1);

            // 部屋の幅と高さをランダムに設定（面積に収める）
            int roomWidth = (int)Math.Sqrt(roomArea); // 面積の平方根を基に幅を決定

            int roomHeight = roomArea / roomWidth; // 残りの面積で高さを決定
            roomWidth += 2;
            roomHeight += 2;
            Debug.Log($"調整:w{roomWidth}:h{roomHeight}");

            // 幅と高さが最小面積に収まるよう調整
            if (roomWidth < 1) roomWidth = 1;
            if (roomHeight < 1) roomHeight = 1;

            // 部屋の座標をランダムに設定
            int roomX = random.Next(1, mapWidth - roomWidth - 1);
            int roomY = random.Next(1, mapHeight - roomHeight - 1);

            var newRoom = new Room(roomX, roomY, roomWidth, roomHeight);

            // 部屋同士の重なりがないかを確認
            bool overlaps = false;
            foreach (var room in rooms)
            {
                if (!IsEnoughDistanceBetweenRooms(newRoom, room))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CreateRoom(newRoom);
            }
        }

        Debug.Log($"部屋を{rooms.Count}個作成");

        // 部屋を接続
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var currentRoom = rooms[i];
            var nextRoom = rooms[i + 1];

            int x1 = currentRoom.CenterX;
            int y1 = currentRoom.CenterY;
            int x2 = nextRoom.CenterX;
            int y2 = nextRoom.CenterY;

            if (random.Next(0, 2) == 0)
            {
                CreateHorizontalTunnel(x1, x2, y1);
                CreateVerticalTunnel(y1, y2, x2);
            }
            else
            {
                CreateVerticalTunnel(y1, y2, x1);
                CreateHorizontalTunnel(x1, x2, y2);
            }
        }
    }

    /// <summary>
    /// 部屋同士が十分な距離を保っているかをチェック
    /// </summary>
    private bool IsEnoughDistanceBetweenRooms(Room newRoom, Room existingRoom)
    {
        int minDistance = 1; // 最低1マスの間隔を取る

        // 部屋の外枠の範囲内で、間隔を確保
        for (int y = newRoom.Y - minDistance; y < newRoom.Y + newRoom.Height + minDistance; y++)
        {
            for (int x = newRoom.X - minDistance; x < newRoom.X + newRoom.Width + minDistance; x++)
            {
                // 他の部屋との距離が1マス未満なら重なっていると見なす
                if (x >= existingRoom.X && x < existingRoom.X + existingRoom.Width &&
                    y >= existingRoom.Y && y < existingRoom.Y + existingRoom.Height)
                {
                    return false; // 間隔が十分でない場合
                }
            }
        }
        return true;
    }

    private void CreateRoom(Room room)
    {
        // 部屋の外周を @ で描画
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                // 外周の条件：上、下、左、右の境界
                if (x == room.X || x == room.X + room.Width - 1 || y == room.Y || y == room.Y + room.Height - 1)
                {
                    map[y, x] = DungeonBaseTip.GetChar(DungeonBaseTip.Tip.RoomOuter); // 外周は @
                }
                else
                {
                    map[y, x] = DungeonBaseTip.GetChar(DungeonBaseTip.Tip.RoomInner); // 内部は &（通路）
                }
            }
        }
    }

    private void CreateHorizontalTunnel(int x1, int x2, int y)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
        {
            // もしその位置が部屋内部（&）なら通路を作成しない
            if (!DungeonBaseTip.IsMatch(map[y, x], DungeonBaseTip.Tip.RoomInner))
            {
                map[y, x] = DungeonBaseTip.GetChar(DungeonBaseTip.Tip.Passage);
            }
        }
    }

    private void CreateVerticalTunnel(int y1, int y2, int x)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
        {
            // もしその位置が部屋内部（&）なら通路を作成しない
            if (!DungeonBaseTip.IsMatch(map[y, x], DungeonBaseTip.Tip.RoomInner))
            {
                map[y, x] = DungeonBaseTip.GetChar(DungeonBaseTip.Tip.Passage);
            }
        }
    }

    public void SaveToCsv(string fileName)
    {
        string path = GetSavePath(fileName);

        // フォルダが存在しない場合は作成
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

                    // 最後の列でなければカンマを追加
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
}

public class Room
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public Room(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Intersects(Room other)
    {
        return !(X + Width <= other.X || X >= other.X + other.Width ||
                 Y + Height <= other.Y || Y >= other.Y + other.Height);
    }
}
