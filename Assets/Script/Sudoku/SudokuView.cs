using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text messageText = null;
    [SerializeField]
    private Toggle numberToggle = null;
    public Toggle NumberToggle { get => numberToggle; set => numberToggle = value; }

    [SerializeField]
    private Button generateButton = null;
    public Button GenerateButton { get => generateButton; set => generateButton = value; }

    [SerializeField, Tooltip("1~9の順番とする")]
    private List<Button> numberButton = new List<Button>();

    [SerializeField]
    private List<NumberGrid> gridNumbers = new List<NumberGrid>();

    [SerializeField]
    private Sprite defaultNumberSprite = null;
    [SerializeField]
    private Sprite selectNumberSprite = null;

    [SerializeField]
    private float messageFadeDuration = 2.0f;


    private void Awake()
    {
        foreach (var grid in gridNumbers)
        {
            grid.AllCellClear();
        }
    }

    /// <summary>
    /// グリッドを初期化
    /// </summary>
    public void Initialize(Action<int, int, NumberGrid> onCellClicked)
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                int gridIndex = y * 3 + x;

                if (gridIndex < gridNumbers.Count)
                {
                    gridNumbers[gridIndex].Initialize(x, y);
                    gridNumbers[gridIndex].OnCellClicked = onCellClicked;
                }
            }
        }
    }

    public void ReStart()
    {
        foreach (var grid in gridNumbers)
        {
            grid.ReStart();
        }
    }

    public void SetNumberAction(Action<int> action)
    {
        for (int i = 0; i < numberButton.Count; i++)
        {
            // 1~9に合わせる
            int index = i + 1;
            numberButton[i].onClick.AddListener(() => { 
                action(index);
                ResetGridColors();
                ResetNumberButtons();
                HighlightNumber(index);
            });
        }
    }

    /// <summary>
    /// 指定した数独の 2 次元配列データをグリッドに反映
    /// </summary>
    /// <param name="data">9×9 の数独データ</param>
    public void SetGridData(int[,] data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int gridIndex = GetGridIndex(row, col);
                int cellIndex = GetCellIndex(row, col);

                if (gridIndex >= 0 && cellIndex >= 0)
                {
                    gridNumbers[gridIndex].SetNumber(cellIndex, data[row, col]);
                }
            }
        }
    }

    /// <summary>
    /// 全てのセルをデフォルトに戻す
    /// </summary>
    public void ResetGridColors()
    {
        foreach (var grid in gridNumbers)
        {
            grid.ResetColors();
        }
    }

    public void ResetNumberButtons()
    {
        foreach (var item in numberButton)
        {
            item.image.sprite = defaultNumberSprite;
        }
    }

    /// <summary>
    /// 数独の特定の数字をハイライト
    /// </summary>
    /// <param name="number">ハイライトする数字</param>
    public void HighlightNumber(int number)
    {
        foreach (var grid in gridNumbers)
        {
            grid.HighlightNumber(number);
        }

        int index = number - 1;
        numberButton[index].image.sprite = selectNumberSprite;
    }

    /// <summary>
    /// 数独の 9×9 セルのインデックスから GridNumber とその内部セルのインデックスを取得
    /// </summary>
    private int GetGridIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3); // 3×3 のグリッド位置を計算
    }

    private int GetCellIndex(int row, int col)
    {
        return (row % 3) * 3 + (col % 3); // GridNumber 内のセルインデックスを計算
    }

    public void SetMessage(string message)
    {
        messageText.DOFade(1.0f, 0);
        messageText.text = message.Trim();
        // 徐々に消える
        messageText.DOFade(0.0f, messageFadeDuration);
    }
}