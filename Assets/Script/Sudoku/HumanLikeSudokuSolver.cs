using System;
using System.Diagnostics;

public class HumanLikeSudokuSolver
{
    private int[,] grid = new int[9, 9];   // 数独の解

    private int explorationCount = 0; // 探索回数
    private bool trialErrorNeeded = false; // 仮置き法が必要かどうか

    // 数独問題を生成
    public int[,] Solve(int[,] sudokuPuzzle)
    {
        Array.Copy(sudokuPuzzle, grid, sudokuPuzzle.Length); // 問題盤面をコピー
        explorationCount = 0;
        trialErrorNeeded = false; // 最初は仮置き法は必要ない

        // 最初に候補リストを初期化
        int[,] candidates = new int[9, 9];
        InitializeCandidates(candidates);

        while (true)
        {
            bool progressMade = false;

            // まずは「唯一候補法」を適用する
            progressMade |= ApplySinglePossibility(candidates);
            // もし進展があったら再度チェック
            if (progressMade)
            {
                continue;
            }

            // 進展がなかった場合、仮置き法（試行錯誤）が必要だとログを出力
            if (!trialErrorNeeded)
            {
                trialErrorNeeded = true;
                UnityEngine.Debug.Log("仮置き法（試行錯誤）が必要です。");
            }

            // もし進展がなければ、仮置き法は使わずに解を諦める
            break;
        }

        UnityEngine.Debug.Log($"探索回数: {explorationCount}");
        return grid;
    }

    // 数字候補を初期化
    private void InitializeCandidates(int[,] candidates)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (grid[r, c] == 0)
                {
                    candidates[r, c] = 0b111111111; // 0は全候補が可能
                }
                else
                {
                    candidates[r, c] = (1 << (grid[r, c] - 1)); // 数字の候補は1ビットだけ
                }
            }
        }
    }

    // 唯一候補法
    private bool ApplySinglePossibility(int[,] candidates)
    {
        bool progressMade = false;

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (grid[r, c] == 0)
                {
                    int possibleNumbers = candidates[r, c];
                    if (CountBits(possibleNumbers) == 1)  // 唯一の候補があれば埋める
                    {
                        int number = GetFirstBit(possibleNumbers);
                        grid[r, c] = number;
                        candidates[r, c] = (1 << (number - 1)); // 候補を固定
                        progressMade = true;
                    }
                }
            }
        }

        explorationCount++;
        return progressMade;
    }

    // ビット数を数える
    private int CountBits(int num)
    {
        int count = 0;
        while (num > 0)
        {
            count += num & 1;
            num >>= 1;
        }
        return count;
    }

    // 最初に1のビット位置を取得
    private int GetFirstBit(int num)
    {
        int bit = 0;
        while ((num & 1) == 0)
        {
            num >>= 1;
            bit++;
        }
        return bit + 1;
    }

    // 解が埋まったかチェック
    private bool IsSolved()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (grid[r, c] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
