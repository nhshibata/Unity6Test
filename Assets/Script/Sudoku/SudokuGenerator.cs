using System;
using System.Collections.Generic;
using System.IO;

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
                Console.WriteLine($"Retrying Sudoku generation... (Attempt {retries + 1}): {ex.Message}");
                Array.Clear(grid, 0, grid.Length); // グリッドをリセット
                retries++;
            }
        }

        throw new InvalidOperationException("Failed to generate a valid Sudoku grid after multiple attempts");
    }

    public int[,] GridToHide(int hideCount)
    {
        RemoveNumbers(hideCount);       // 解から数字を取り除いて問題盤面を生成
        HumanLikeSudokuSolver human = new HumanLikeSudokuSolver();
        human.Solve(hideGrid);
        return hideGrid;
    }

    // 数独を解く
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

    // 問題盤面を生成（難易度に応じて穴をあける数を調整）
    private void RemoveNumbers(int hideCount)
    {
        Array.Copy(grid, hideGrid, grid.Length); // 解をコピー

        int holesPlaced = 0;
        while (holesPlaced < hideCount)
        {
            int row = random.Next(0, 9);
            int col = random.Next(0, 9);

            // すでに穴があいている場合はスキップ
            if (hideGrid[row, col] == 0)
                continue;

            // 数字を隠す
            hideGrid[row, col] = 0;
            holesPlaced++;
        }
    }

    // その数字を置いて安全かどうかチェック
    private bool IsSafe(int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (grid[row, i] == num || grid[i, col] == num)
            {
                return false;
            }
        }

        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                if (grid[r, c] == num)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 盤面がすべて埋まっているかチェック
    private bool IsFilled()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (grid[i, j] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // リストをシャッフルする
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


public static class ListExtensions
{
    // List<T>をシャッフルする拡張メソッド
    public static void Shuffle<T>(this IList<T> list)
    {
        Random rand = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
