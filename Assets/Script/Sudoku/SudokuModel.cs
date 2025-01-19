using R3;
using System;
using UnityEngine;
using static SudokuConfig;

[Serializable]
public class SudokuModel
{
    [SerializeField]
    private SudokuGenerator generator = new SudokuGenerator();

    [SerializeField]
    private ReactiveProperty<DifficultyLevel> difficulty = new ReactiveProperty<DifficultyLevel>();
    public ReactiveProperty<DifficultyLevel> Difficulty { get => difficulty; set => difficulty = value; }

    private ReactiveProperty<float> timer = new ReactiveProperty<float>(0.0f);
    public ReactiveProperty<float> Timer => timer;

    private ReactiveProperty<int> selectNumber = new ReactiveProperty<int>(1);
    public ReactiveProperty<int> SelectNumber => selectNumber;

    private ReactiveProperty<int[,]> hideGrid = new ReactiveProperty<int[,]>();
    public ReadOnlyReactiveProperty<int[,]> HideGrid => hideGrid;

    private Subject<Unit> onComplete = new Subject<Unit>();
    public Observable<Unit> OnComplete => onComplete;

    private ReactiveProperty<bool> isGame = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> IsGame { get => isGame; set => isGame = value; }

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
        grid[x, y] = number;
        hideGrid.Value = grid;

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
        isGame.Value = true;
        return true;
    }
}
