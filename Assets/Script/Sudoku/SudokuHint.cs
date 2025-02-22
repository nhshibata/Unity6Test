using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 数独の空きセルに入る候補を取得
/// </summary>
public class SudokuHint
{
    private const int SIZE = 9;

    // 1-9 の候補を全て立てたビットフラグ
    private const int BIT_FLAG = 0b111111111;


    /// <summary>
    /// 指定されたセルに入る可能な数字をビットフラグで取得
    /// </summary>
    public int GetPossibleValues(int[,] grid, int row, int col, int checkValue = BIT_FLAG)
    {
        int possibleValues = checkValue; 

        // すでに埋まっている場合は候補がない
        if (grid[row, col] != 0)
            return 0;

        // ① 同じ行を確認
        for (int c = 0; c < SIZE; c++)
        {
            int value = grid[row, c];
            if (value != 0)
            {
                possibleValues &= ~(1 << (value - 1));
            }
        }

        // ② 同じ列を確認
        for (int r = 0; r < SIZE; r++)
        {
            int value = grid[r, col];
            if (value != 0)
            {
                possibleValues &= ~(1 << (value - 1));
            }
        }

        // ③ 3x3のボックス内を確認
        int boxStartRow = (row / 3) * 3;
        int boxStartCol = (col / 3) * 3;
        for (int r = boxStartRow; r < boxStartRow + 3; r++)
        {
            for (int c = boxStartCol; c < boxStartCol + 3; c++)
            {
                int value = grid[r, c];
                if (value != 0)
                {
                    possibleValues &= ~(1 << (grid[r, c] - 1));
                }
            }
        }

        return possibleValues;
    }

    public int[,] RemoveGrid(int[,] grid)
    {
        int[,] hintGrid = (int[,])grid.Clone();

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                // 空いているセルに対して
                if (hintGrid[row, col] != 0)
                    continue;

                // 指定された数字のみ候補として探索
                hintGrid[row, col] = GetPossibleValues(hintGrid, row, col, hintGrid[row, col]);
            }
        }

        return hintGrid;
    }

    /// <summary>
    /// 候補となる数字を表示
    /// </summary>
    public int[,] GetHint(int[,] grid, int number)
    {
        int[,] hintGrid = (int[,])grid.Clone();

        // 指定された数字が1から9の範囲内であるか確認
        if (number < 1 || number > 9)
        {
            throw new ArgumentException("数字は1から9の範囲で指定してください");
        }

        Debug.Log($"ヒント{number}を表示");

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                // 空いているセルに対して
                if (hintGrid[row, col] != 0)
                    continue;

                // 指定された数字のみ候補として探索
                int possibleValues = GetPossibleValues(hintGrid, row, col);
                if ((possibleValues & (1 << (number - 1))) > 0) // 指定された数字が候補に含まれているか
                {
                    Debug.Log($"row{row},{col} : {number}");
                    hintGrid[row, col] |= (1 << (number - 1));
                }
            }
        }

        return hintGrid;
    }

    public int[,] GetAllHint(int[,] grid)
    {
        int[,] hintGrid = (int[,])grid.Clone();

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                // 空いているセルに対して
                if (hintGrid[row, col] != 0)
                    continue;

                // 指定された数字のみ候補として探索
                int possibleValues = GetPossibleValues(hintGrid, row, col);
                hintGrid[row, col] = possibleValues;
            }
        }

        return hintGrid;
    }

}
