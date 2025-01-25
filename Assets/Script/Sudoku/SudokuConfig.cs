using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SudokuConfig
{
    /// <summary>
    /// 難易度を表す列挙型
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    public int Seed { get; private set; }

    public int HiddenCount { get; private set; }

    public DifficultyLevel Difficulty { get; private set; }

    public DateTime Timestamp { get; private set; }

    public string FormattedTimestamp { get; private set; }

    public float ClearTime { get; private set; }

    public int Score { get; private set; }


    public SudokuConfig(int seed, int hiddenCount, DifficultyLevel difficulty, float clearTime = 0, int score = 0)
    {
        Seed = seed;
        HiddenCount = hiddenCount;
        Difficulty = difficulty;
        Timestamp = DateTime.Now;
        FormattedTimestamp = Timestamp.ToString("yyyyMMdd_HHmmss");
        ClearTime = clearTime;
        Score = score;
    }

    public override string ToString()
    {
        return $"Seed: {Seed}, HiddenCount: {HiddenCount}, Difficulty: {Difficulty}, ClearTime: {ClearTime} sec, Score: {Score}, Timestamp: {FormattedTimestamp}";
    }

    public static int GetHiddenCountByDifficulty(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy: return 30;
            case DifficultyLevel.Medium: return 40;
            case DifficultyLevel.Hard: return 50;
            case DifficultyLevel.Expert: return 60;
            default: return 40;
        }
    }
}

public static class SudokuConfigManager
{
    private static string SaveDirectory => Application.persistentDataPath;

    public static void SaveConfig(SudokuConfig config)
    {
        // ファイル名にフォーマット済み日時を含める
        string fileName = $"sudoku_config_{config.FormattedTimestamp}.json";
        string savePath = Path.Combine(SaveDirectory, fileName);

        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"Config saved to: {savePath}");
    }

    public static SudokuConfig LoadConfig(string fileName)
    {
        string filePath = Path.Combine(SaveDirectory, fileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SudokuConfig>(json);
        }

        Debug.LogWarning($"No config file found at: {filePath}");
        return null;
    }
}
