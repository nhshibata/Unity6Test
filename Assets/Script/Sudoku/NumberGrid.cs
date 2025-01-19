using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberGrid : MonoBehaviour
{
    private static readonly int CELL_SIZE = 3;

    [SerializeField]
    private Color defaultColor = Color.black;

    [SerializeField]
    private Color highlightColor = Color.blue;

    [SerializeField]
    private List<SudokuButton> cells = new List<SudokuButton>();

    // (x, y) を通知するコールバック
    public Action<int, int, NumberGrid> OnCellClicked;

    // この GridNumber の X 座標
    private int gridX;
    // この GridNumber の Y 座標
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
            int localIndex = i; // キャプチャ対策でローカル変数に格納
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
            cell.Text.color = defaultColor;
        }
    }

    /// <summary>
    /// 指定したインデックスに数字を設定し、任意で色を指定
    /// </summary>
    /// <param name="index">セルのインデックス</param>
    /// <param name="number">設定する数字</param>
    /// <param name="size">グリッドのサイズ</param>
    /// <param name="color">任意の色 (nullの場合はデフォルト色)</param>
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
        }
    }

    public void SetCandidateNumber(int index, int number)
    {
        var cell = cells[index];
        cell.SetCandidateNumber(number);
    }

    /// <summary>
    /// 指定した数字に対応するセルをハイライト
    /// </summary>
    /// <param name="number">ハイライトする数字</param>
    public void HighlightNumber(int number)
    {
        foreach (var item in cells)
        {
            if (int.TryParse(item.Text.text, out int cellNumber) && cellNumber == number)
            {
                item.Text.color = highlightColor;
            }
        }
    }

    /// <summary>
    /// 全てのセルの色をデフォルトに戻す
    /// </summary>
    public void ResetColors()
    {
        foreach (var item in cells)
        {
            item.Text.color = defaultColor;
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
            item.Text.color = defaultColor;
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
