using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;

public class SudokuGenerator
{
    private Random random;

    private int[,] grid = new int[9, 9];
    public int[,] Grid { get => grid; }
    
    private int[,] hideGrid = new int[9, 9];
    public int[,] HideGrid { get => hideGrid; }


    public int[,] GenerateSudoku(int seed)
    {
        // シード値を設定
        random = new Random(seed);

        // 初期盤面生成のリトライ回数
        const int maxRetries = 5;
        int retries = 0;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                grid[i, j] = 0;
                hideGrid[i, j] = 0;
            }
        }

        while (retries < maxRetries)
        {
            try
            {
                // 初期盤面を生成
                FillGrid();

                // 正常に生成できた場合
                ExportGridToCsv(grid, "Assets/sudoku_grid.csv");
                return grid;
            }
            catch (StackOverflowException)
            {
                // 再帰が無限ループに陥った場合
                Console.WriteLine($"Retrying Sudoku generation... (Attempt {retries + 1})");
                Array.Clear(grid, 0, grid.Length); // グリッドをリセット
                retries++;
            }
        }

        // 最大リトライ回数を超えた場合
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
        // ランダムに完全なナンプレを生成
        FillDiagonal();
        FillRemaining(0, 3);
    }

    private void FillDiagonal()
    {
        for (int i = 0; i < 9; i += 3)
        {
            FillBox(i, i);
        }
    }

    private void FillBox(int row, int col)
    {
        List<int> numbers = Enumerable.Range(1, 9).ToList();
        Random rng = new Random();
        numbers = numbers.OrderBy(_ => rng.Next()).ToList(); // ランダムにシャッフル

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[row + i, col + j] != 0) continue;

                bool placed = false;
                foreach (int num in numbers)
                {
                    if (IsSafeInBox(row, col, num))
                    {
                        grid[row + i, col + j] = num;
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    // 全ての候補が失敗した場合、グリッドの生成をリトライ
                    throw new Exception("Failed to place a number in the box");
                }
            }
        }
    }

    private bool FillRemaining(int i, int j)
    {
        int attemptCount = 0;
        const int maxAttempts = 100; // 試行回数の上限

        // 行列を超えた場合
        if (j >= 9 && i < 8)
        {
            i++;
            j = 0;
        }
        if (i >= 9 && j >= 9)
            return true;

        if (i < 3 && j < 3)
            j = 3;
        else if (i < 6 && j == (i / 3) * 3)
            j += 3;
        else if (i >= 6 && j == 6)
        {
            i++;
            j = 0;
            if (i >= 9)
                return true;
        }

        // 数字をランダムに試してみる
        for (int num = 1; num <= 9; num++)
        {
            // 行・列・ボックスに数字が安全かどうかをチェック
            if (IsSafe(i, j, num))
            {
                grid[i, j] = num;

                // 次のセルへ進む
                if (FillRemaining(i, j + 1))
                    return true;

                // うまくいかなければ元に戻す
                grid[i, j] = 0;
            }

            // 試行回数をカウント
            attemptCount++;
            if (attemptCount >= maxAttempts)
                return false; // 試行回数超過で失敗を返す
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

    /// <summary>
    /// グリッドをCSV形式で出力する
    /// </summary>
    /// <param name="grid">出力したいグリッド (grid または hideGrid)</param>
    /// <param name="filePath">保存先のファイルパス</param>
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
                    row = "---------,";
                    writer.WriteLine(row);

                }
            }
        }
    }

    /// <summary>
    /// 指定した行を取得する
    /// </summary>
    /// <param name="grid">グリッド</param>
    /// <param name="rowIndex">行インデックス</param>
    /// <returns>指定した行の値の配列</returns>
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
