using System;
using UnityEngine;
using static SudokuConfig;

[Serializable]
public class SudokuModel
{
    [SerializeField]
    private SudokuGenerator generator = new SudokuGenerator();

    [SerializeField]
    private DifficultyLevel difficulty;

    private int selectNumber = 1;
    public int SelectNumber { get => selectNumber; set => selectNumber = value; }

    private int seed;
    
    private int[,] hideGrid;
    public int[,] HideGrid { get => hideGrid; }

    public Action OnComplete { get; set; }


    public void Generate()
    {
        seed = (int)DateTime.Now.Ticks;
        generator.GenerateSudoku(seed);
        hideGrid = generator.GridToHide(SudokuConfig.GetHiddenCountByDifficulty(difficulty));
    }

    public bool CheckNumber(int x, int y)
    {
        Debug.Log($"x{x},y{y}:grid{generator.Grid[y, x]}:select{selectNumber}");
        return (generator.Grid[y, x] == selectNumber);
    }

    public void Save()
    {
        SudokuConfig config = new SudokuConfig(seed, SudokuConfig.GetHiddenCountByDifficulty(difficulty), difficulty);
        SudokuConfigManager.SaveConfig(config);
    }

    public void SetNumber(int x, int y, int number)
    {
        hideGrid[x,y] = number;
        if(IsComplete())
        {
            OnComplete?.Invoke();
        }
    }

    public bool IsComplete()
    {
        foreach (var item in hideGrid)
        {
            if(item == 0)
                return false;
        }
        return true;
    }

}
