using System;
using System.Collections.Generic;

public class SudokuGenerator
{
    private Random random;
    private int[,] grid = new int[9, 9];
    public int[,] Grid { get => grid; }

    private int[,] hideGrid = new int[9, 9];
    public int[,] HideGrid { get => hideGrid; }


    // 数独の回答パターンを生成（バックトラッキング）
    public int[,] GenerateSudoku(int seed)
    {
        random = new Random(seed);
        const int maxRetries = 10;
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                // 初期盤面を生成
                FillGrid();

                // 正常に生成できた場合
                return grid;
            }
            catch (Exception ex)
            {
                // エラーが発生した場合の処理
                UnityEngine.Debug.Log($"Retrying Sudoku generation... (Attempt {retries + 1}): {ex.Message}");
                Array.Clear(grid, 0, grid.Length); // グリッドをリセット
                retries++;
            }
        }

        throw new InvalidOperationException("Failed to generate a valid Sudoku grid after multiple attempts");
    }

    public int[,] GridToHide(int hideCount)
    {
        hideGrid = RemoveNumbers(hideCount);       // 解から数字を取り除いて問題盤面を生成
        HumanLikeSudokuSolver human = new HumanLikeSudokuSolver();
        human.Solve(hideGrid);
        return hideGrid;
    }

    private bool FillGrid()
    {
        var numbers = new List<int>(System.Linq.Enumerable.Range(1, 9));
        for (int i = 0; i < 81; i++)
        {
            int row = i / 9;
            int col = i % 9;
            if (grid[row, col] == 0)
            {
                Shuffle(numbers); // ランダムに数字をシャッフル
                foreach (var number in numbers)
                {
                    if (IsSafe(row, col, number))
                    {
                        grid[row, col] = number;
                        if (IsFilled())
                        {
                            return true; // すべてのセルが埋まったらtrueを返す
                        }
                        if (FillGrid()) // 再帰呼び出しが成功した場合、trueを返す
                        {
                            return true;
                        }
                        grid[row, col] = 0; // 戻す（バックトラッキング）
                    }
                }
                return false; // 解が見つからなかった場合falseを返す
            }
        }
        return true; // すべてのセルが埋まった場合、trueを返す
    }

    public int[,] RemoveNumbers(int holes)
    {
        int[,] gridCopy = (int[,])grid.Clone();
        List<(int, int)> cells = new List<(int, int)>();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells.Add((row, col));
            }
        }

        ShuffleList(cells);
        int removed = 0;

        foreach (var (row, col) in cells)
        {
            if (removed >= holes)
            {
                break;
            }

            int originalValue = gridCopy[row, col];
            gridCopy[row, col] = 0;

            if (!HasUniqueSolution(gridCopy))
            {
                gridCopy[row, col] = originalValue;
            }
            else
            {
                removed++;
            }
        }

        UnityEngine.Debug.Log($"空けた数{removed}");

        return gridCopy;
    }

    private bool IsSafe(int row, int col, int num)
    {
        // 行チェック
        for (int c = 0; c < 9; c++)
        {
            if (grid[row, c] == num)
            {
                return false;
            }
        }

        // 列チェック
        for (int r = 0; r < 9; r++)
        {
            if (grid[r, col] == num)
            {
                return false;
            }
        }

        // ボックスチェック
        int boxStartRow = 3 * (row / 3);
        int boxStartCol = 3 * (col / 3);
        for (int r = boxStartRow; r < boxStartRow + 3; r++)
        {
            for (int c = boxStartCol; c < boxStartCol + 3; c++)
            {
                if (grid[r, c] == num)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsFilled()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void Shuffle(List<int> numbers)
    {
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }
    }

    private bool HasUniqueSolution(int[,] grid)
    {
        int solutionCount = 0;

        void Solve()
        {
            for (int i = 0; i < 81; i++)
            {
                int row = i / 9;
                int col = i % 9;
                if (grid[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsSafeForGrid(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            Solve();
                            grid[row, col] = 0;
                        }
                    }
                    return;
                }
            }
            solutionCount++;
            if (solutionCount > 1)
            {
                return;
            }
        }

        Solve();
        return solutionCount == 1;
    }

    private bool IsSafeForGrid(int[,] grid, int row, int col, int num)
    {
        // 行チェック
        for (int c = 0; c < 9; c++)
        {
            if (grid[row, c] == num)
            {
                return false;
            }
        }

        // 列チェック
        for (int r = 0; r < 9; r++)
        {
            if (grid[r, col] == num)
            {
                return false;
            }
        }

        // ボックスチェック
        int boxStartRow = 3 * (row / 3);
        int boxStartCol = 3 * (col / 3);
        for (int r = boxStartRow; r < boxStartRow + 3; r++)
        {
            for (int c = boxStartCol; c < boxStartCol + 3; c++)
            {
                if (grid[r, c] == num)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void ShuffleList<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
