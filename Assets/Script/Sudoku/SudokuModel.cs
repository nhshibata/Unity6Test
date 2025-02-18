using R3;
using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static SudokuConfig;

[Serializable]
public class SudokuModel
{
    [SerializeField]
    private SudokuGenerator generator = new SudokuGenerator();

    [SerializeField]
    private SudokuHint hint = new SudokuHint();

    [SerializeField]
    private ReactiveProperty<DifficultyLevel> difficulty = new ReactiveProperty<DifficultyLevel>();
    public ReactiveProperty<DifficultyLevel> Difficulty { get => difficulty; set => difficulty = value; }

    private ReactiveProperty<float> timer = new ReactiveProperty<float>(0.0f);
    public ReactiveProperty<float> Timer => timer;

    private ReactiveProperty<int> selectNumber = new ReactiveProperty<int>(1);
    public ReactiveProperty<int> SelectNumber => selectNumber;

    private ReactiveProperty<int[,]> hideGrid = new ReactiveProperty<int[,]>();
    public ReadOnlyReactiveProperty<int[,]> HideGrid => hideGrid;
    
    private ReactiveProperty<int[,]> possibleGrid = new ReactiveProperty<int[,]>();
    public ReactiveProperty<int[,]> PossibleGrid { get => possibleGrid; }

    private Subject<Unit> onComplete = new Subject<Unit>();
    public Observable<Unit> OnComplete => onComplete;

    private ReactiveProperty<bool> isGame = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> IsGame { get => isGame; set => isGame = value; }

    // TODO:ミス回数を記録
    // TODO:スコアを記録
    // TODO:ヒント押下回数

    /// <summary>
    /// 現在のシード値
    /// </summary>
    private int seed;


    /// <summary>
    /// 非同期で数独を生成
    /// </summary>
    public void Generate()
    {
        isGame.Value = true;
        timer.Value = 0.0f;
        seed = (int)DateTime.Now.Ticks;

        generator.GenerateSudoku(seed);
        hideGrid.Value = generator.GridToHide(SudokuConfig.GetHiddenCountByDifficulty(difficulty.Value));
        possibleGrid.Value = (int[,])hideGrid.Value.Clone();
    }

    /// <summary>
    /// 数字をチェックする
    /// </summary>
    public bool CheckNumber(int x, int y)
    {
        Debug.Log($"x{x},y{y}:grid{generator.Grid[y, x]}:select{selectNumber.Value}");
        return (generator.Grid[y, x] == selectNumber.Value);
    }

    public void TimerUpdate()
    {
        if (!isGame.Value)
            return;

        timer.Value += Time.deltaTime;
    }

    /// <summary>
    /// コンフィグを保存する
    /// </summary>
    public void Save()
    {
        SudokuConfig config = new SudokuConfig(seed, SudokuConfig.GetHiddenCountByDifficulty(difficulty.Value), difficulty.Value, timer.Value);
        SudokuConfigManager.SaveConfig(config);
    }

    /// <summary>
    /// グリッドに数字を設定
    /// </summary>
    public void SetNumber(int x, int y, int number)
    {
        var grid = hideGrid.Value;
        grid[y, x] = number;
        hideGrid.Value = grid;
        SetPossibleGrid(x, y, number);

        if (IsComplete())
        {
            onComplete.OnNext(Unit.Default); // 完了通知
        }
    }

    /// <summary>
    /// 全て埋まったかをチェック
    /// </summary>
    public bool IsComplete()
    {
        foreach (var item in hideGrid.Value)
        {
            if (item == 0)
                return false;
        }
        isGame.Value = false;
        return true;
    }

    public void SetPossibleGrid(int x, int y, int number)
    {
        var grid = possibleGrid.Value;
        grid[y, x] = number;
        grid = hint.RemoveGrid(grid);
        possibleGrid.Value = grid;
    }

    /// <summary>
    /// 数字候補を更新
    /// </summary>
    public void UpdatePossibleGrid(int x, int y, int number)
    {
        var grid = possibleGrid.Value;
        int candidateBits = grid[y, x];
        int bit = 1 << (number - 1);

        if ((candidateBits & bit) != 0)
        {
            candidateBits &= ~bit;
        }
        else
        {
            candidateBits |= bit;
        }

        grid[y, x] = candidateBits;
        possibleGrid.Value = grid;
    }

    public int[,] GetHintGenerate()
    {
        int hintNumber = UnityEngine.Random.Range(1, 9);
        var grid = hint.GetHint(possibleGrid.Value, hintNumber);
        //grid = hint.GetAllHint(possibleGrid.Value);
        possibleGrid.Value = grid;
        return grid;
    }

}
