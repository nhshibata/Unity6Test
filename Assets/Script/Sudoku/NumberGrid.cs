using System;
using System.Collections.Generic;
using UnityEngine;

public class NumberGrid : MonoBehaviour
{
    public enum ColorIndex
    {
        Default,
        Highlight,
        ClearHighlight,
    }

    private static readonly int CELL_SIZE = 3;
    private static readonly Color defaultColor = Color.black;
    private static readonly Color highlightColor = Color.blue * 0.9f + new Color(0, 0, 0, 1);
    private static readonly Color clearHighlightColor = Color.green * 0.9f + new Color(0, 0, 0, 1);

    [SerializeField]
    private List<SudokuButton> cells = new List<SudokuButton>();

    /// <summary>
    /// (x, y) を通知するコールバック
    /// </summary>
    public Action<int, int, NumberGrid> OnCellClicked;

    /// <summary>
    /// この GridNumber の X 座標
    /// </summary>
    private int gridX;
    /// <summary>
    /// この GridNumber の Y 座標
    /// </summary>
    private int gridY;


    /// <summary>
    /// GridNumber のインデックスを設定
    /// </summary>
    /// <param name="x">グリッドの X 座標</param>
    /// <param name="y">グリッドの Y 座標</param>
    public void Initialize(int x, int y)
    {
        gridX = x;
        gridY = y;

        for (int i = 0; i < cells.Count; i++)
        {
            int localIndex = i; // キャプチャ対策
            cells[localIndex].Button.onClick.AddListener(() =>
            {
                // ボタンが押されたときに (x, y) を計算してコールバック
                int cellX = gridX * CELL_SIZE + localIndex % CELL_SIZE;
                int cellY = gridY * CELL_SIZE + localIndex / CELL_SIZE;

                OnCellClicked?.Invoke(cellX, cellY, this);
            });
        }
    }

    public void ReStart()
    {
        foreach (var cell in cells)
        {
            cell.SetCandidateEnable(true);
            cell.Text.text = " ";
            cell.Text.color = defaultColor;
        }
    }

    /// <summary>
    /// 指定したインデックスに数字を設定
    /// </summary>
    public void SetNumber(int index, int number)
    {
        var cell = cells[index];
        if (number <= 0 || number > 9)
        {
            cell.Text.text = " ";
            cell.SetCandidateEnable(true);
            cell.Button.image.raycastTarget = true;
        }
        else
        {
            cell.Text.text = number.ToString();
            cell.SetCandidateEnable(false);
            cell.Button.image.raycastTarget = false;
            HighlightNumber(number, NumberGrid.ColorIndex.Highlight);
        }
    }

    public void SetCandidateNumber(int index, int number)
    {
        var cell = cells[index];
        cell.SetCandidateNumber(number);
    }
    
    public void UpdateCandidateNumber(int index, int number)
    {
        var cell = cells[index];
        cell.UpdateCandidateNumber(number);
    }

    public bool MatchNumber(int number, Action<TMPro.TMP_Text> onMatchFound)
    {
        foreach (var item in cells)
        {
            if (int.TryParse(item.Text.text, out int cellNumber) && cellNumber == number)
            {
                onMatchFound?.Invoke(item.Text);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 指定した数字に対応するセルをハイライト
    /// </summary>
    public void HighlightNumber(int number, ColorIndex colorIndex)
    {
        MatchNumber(number, (text) =>
        {
            if (int.TryParse(text.text, out int cellNumber) && cellNumber == number)
            {
                text.color = GetColor(colorIndex);
            }
        });
    }

    private static Color GetColor(ColorIndex colorIndex) => colorIndex switch
    {
        ColorIndex.Default => defaultColor,
        ColorIndex.Highlight => highlightColor,
        ColorIndex.ClearHighlight => clearHighlightColor,
        _ => throw new NotImplementedException(),
    };

    public void ResetColors()
    {
        foreach (var item in cells)
        {
            if (item.Text.color == GetColor(ColorIndex.ClearHighlight))
                continue;
            item.Text.color = GetColor(ColorIndex.Default);
        }
    }

    /// <summary>
    /// 全てのセルをクリアし、色をデフォルトに戻す
    /// </summary>
    public void AllCellClear()
    {
        foreach (var item in cells)
        {
            item.Text.text = " ";
            item.Text.color = GetColor(ColorIndex.Default);
        }
    }

    public static int PosToIndex(int x, int y)
    {
        // 3x3 サブグリッド内のローカル座標を計算
        int localX = x % CELL_SIZE;
        int localY = y % CELL_SIZE;
        return localX + localY * CELL_SIZE;
    }

}
