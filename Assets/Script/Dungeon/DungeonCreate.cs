using System;
using UnityEngine;

public class DungeonCreate : MonoBehaviour
{
    public void Start()
    {
        var generator = new DungeonGenerator();
        generator.GenerateDungeon();
        generator.SaveToCsv("dungeon.csv");
        Console.WriteLine("ダンジョンを生成し、CSVに保存しました。");
    }
}
