using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class SudokuGenerator
{
    private Random random;
    private int[,] grid = new int[9, 9];
    public int[,] Grid { get => grid; }

    private int[,] hideGrid = new int[9, 9];
    public int[,] HideGrid { get => hideGrid; }

    public int[,] GenerateSudoku(int seed)
    {
        random = new Random(seed);
        const int maxRetries = 5;
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                // 初期盤面を生成
                FillGrid();

#if UNITY_EDITOR
                // 正常に生成できた場合
                ExportGridToCsv(grid, "Assets/sudoku_grid.csv");
#endif
                return grid;
            }
            catch (Exception ex)
            {
                // エラーが発生した場合の処理
                Console.WriteLine($"Retrying Sudoku generation... (Attempt {retries + 1}): {ex.Message}");
                Array.Clear(grid, 0, grid.Length); // グリッドをリセット
                retries++;
            }
        }

        throw new InvalidOperationException("Failed to generate a valid Sudoku grid after multiple attempts");
    }

    public int[,] GridToHide(int hiddenCount)
    {
        // 数字を隠す
        HideNumbers(hiddenCount);
        return hideGrid;
    }

    private void FillGrid()
    {
        FillDiagonal();
        FillRemainingGrid();
    }

    private void FillDiagonal()
    {
        // 3x3のボックスを埋める
        for (int i = 0; i < 9; i += 3)
        {
            FillBox(i, i);
        }
    }

    private void FillBox(int row, int col)
    {
        List<int> numbers = Enumerable.Range(1, 9).ToList();
        numbers = numbers.OrderBy(_ => random.Next()).ToList(); // ランダムにシャッフル

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[row + i, col + j] != 0) continue;

                foreach (int num in numbers)
                {
                    if (IsSafeInBox(row, col, num) && IsSafeInRow(row + i, num) && IsSafeInCol(col + j, num))
                    {
                        grid[row + i, col + j] = num;
                        break;
                    }
                }
            }
        }
    }

    private bool FillRemainingGrid()
    {
        List<(int row, int col)> emptyCells = new List<(int row, int col)>();

        // 空いているセルの位置をリストに追加
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (grid[i, j] == 0)
                    emptyCells.Add((i, j));
            }
        }

        return TryFillCells(emptyCells, 0);
    }

    private bool TryFillCells(List<(int row, int col)> emptyCells, int index)
    {
        if (index == emptyCells.Count) return true;

        var (row, col) = emptyCells[index];
        List<int> numbers = Enumerable.Range(1, 9).OrderBy(_ => random.Next()).ToList();

        foreach (int num in numbers)
        {
            if (IsSafe(row, col, num))
            {
                grid[row, col] = num;

                if (TryFillCells(emptyCells, index + 1))
                    return true;

                grid[row, col] = 0; // 戻す
            }
        }

        return false;
    }

    private bool IsSafe(int i, int j, int num)
    {
        return IsSafeInRow(i, num) && IsSafeInCol(j, num) && IsSafeInBox(i - i % 3, j - j % 3, num);
    }

    private bool IsSafeInRow(int i, int num)
    {
        for (int j = 0; j < 9; j++)
            if (grid[i, j] == num)
                return false;
        return true;
    }

    private bool IsSafeInCol(int j, int num)
    {
        for (int i = 0; i < 9; i++)
            if (grid[i, j] == num)
                return false;
        return true;
    }

    private bool IsSafeInBox(int boxStartRow, int boxStartCol, int num)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[boxStartRow + i, boxStartCol + j] == num)
                    return false;
            }
        }
        return true;
    }

    private void HideNumbers(int count)
    {
        Array.Copy(grid, hideGrid, 9 * 9);

        for (int i = 0; i < count;)
        {
            int cellId = random.Next(0, 81);
            int row = cellId / 9;
            int col = cellId % 9;

            if (hideGrid[row, col] != 0)
            {
                hideGrid[row, col] = 0;
                i++;
            }
        }
    }

    public void ExportGridToCsv(int[,] grid, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < 9; i++)
            {
                string row = string.Join(",", GetRow(grid, i));
                writer.WriteLine(row);

                if ((i + 1) % 3 == 0)
                {
                    row = "---------,"; // 3行ごとに区切りを入れる
                    writer.WriteLine(row);
                }
            }
        }
    }

    private int[] GetRow(int[,] grid, int rowIndex)
    {
        int[] row = new int[9];
        for (int j = 0; j < 9; j++)
        {
            row[j] = grid[rowIndex, j];
        }
        return row;
    }
}
