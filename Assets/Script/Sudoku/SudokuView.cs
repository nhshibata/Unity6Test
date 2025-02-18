using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SudokuView : MonoBehaviour
{
    [SerializeField]
    private Image blackPanel = null;
    [SerializeField]
    private TMP_Text timer = null;
    [SerializeField]
    private TMP_Text messageText = null;

    [SerializeField]
    private Toggle numberToggle = null;
    public Toggle NumberToggle { get => numberToggle; }

    [SerializeField]
    private Button generateButton = null;
    public Button GenerateButton { get => generateButton; }

    [SerializeField]
    private Button hintButton = null;

    [SerializeField]
    private Dropdown dropdown = null;
    public Dropdown Dropdown { get => dropdown; }

    [SerializeField]
    private List<EmotionCharacterSprite> characterSpriteManager = new List<EmotionCharacterSprite>();

    [SerializeField, Tooltip("記入ボタン（1~9の順番とする）")]
    private List<Button> numberButton = new List<Button>();

    [SerializeField, Tooltip("表示グリッド（左上から右下の順に格納）")]
    private List<NumberGrid> gridNumbers = new List<NumberGrid>();

    [Header("Parameter")]
    [SerializeField]
    private float messageFadeDuration = 2.0f;

    [SerializeField]
    private Color defaultNumberColor;
    [SerializeField]
    private Color selectNumberColor;
    [SerializeField]
    private Color completeNumberColor;

    private int characterIndex = 0;

    public Action OnHintClick { get; set; }


    private void Awake()
    {
        foreach (var grid in gridNumbers)
        {
            grid.AllCellClear();
        }

        SetMessage("generate!", false);
        characterSpriteManager.ForEach(obj=>obj.gameObject.SetActive(false));

        hintButton.onClick.AddListener(() => { OnHintClick?.Invoke(); });
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

        for (int i = 1; i <= 9; ++i)
        {
            CheckNumbers(i);
        }
    }

    public void ReStart()
    {
        foreach (var grid in gridNumbers)
        {
            grid.ReStart();
        }
        foreach (var button in numberButton)
        {
            button.image.raycastTarget = true;
        }

        ChangeCharacter();

        // 入力防止
        blackPanel.raycastTarget = true;
        DOVirtual.DelayedCall(1.0f, () => { blackPanel.raycastTarget = false; },false);
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
            item.image.color = defaultNumberColor;
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
    /// 数独の特定の数字をハイライト
    /// </summary>
    /// <param name="number">ハイライトする数字</param>
    public void HighlightNumber(int number)
    {
        foreach (var grid in gridNumbers)
        {
            grid.HighlightNumber(number, NumberGrid.ColorIndex.Highlight);
        }

        int index = number - 1;
        numberButton[index].image.color = selectNumberColor;
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

    public void SetMessage(string message, bool isFade)
    {
        messageText?.DOKill();
        messageText.DOFade(1.0f, 0);
        messageText.text = message.Trim();
        // 徐々に消える
        if(isFade)
            messageText.DOFade(0.0f, messageFadeDuration);
        else
            messageText.DOKill();
    }

    public void SetTimerText(float time)
    {
        // 秒数を分と秒に変換
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        // mm:ss 形式でテキストに反映
        timer.text = $"{minutes:00}:{seconds:00}";
    }

    public void SetAllCandidateNumber(int[,] data)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int gridIndex = GetGridIndex(row, col);
                int cellIndex = GetCellIndex(row, col);

                if (gridIndex >= 0 && cellIndex >= 0)
                {
                    gridNumbers[gridIndex].UpdateCandidateNumber(cellIndex, data[row, col]);
                }
            }
        }
    }

    public void ChangeCharacter()
    {
        int index = characterIndex;

        characterSpriteManager[characterIndex].ResetToDefaultCharacter();
        characterSpriteManager.ForEach(character => character.RectAnim.StopAnimation());

        if (characterSpriteManager.Count > 0)
        {
            do
            {
                index = UnityEngine.Random.Range(0, characterSpriteManager.Count);
            } while (index == characterIndex);
        }

        characterSpriteManager[characterIndex].StartBlackFade(
            null,
            () =>
            {
                characterSpriteManager[characterIndex].gameObject.SetActive(false);
                characterSpriteManager[index].gameObject.SetActive(true);
                characterSpriteManager[index].StartWhiteFade(
                    (image) =>
                    {
                        image.color = Color.black;
                    },
                    () =>
                    {
                        characterIndex = index;
                    });
            });
    }

    public void StartSuccessEffect(int number)
    {
        characterSpriteManager[characterIndex].ChangeToRandomCharacter(EmotionCharacterSprite.Emotion.Positive);
        characterSpriteManager[characterIndex].RectAnim.StartAnimation();

        int matchCount = 0;
        foreach (var grid in gridNumbers)
        {
            if (grid.MatchNumber(number, null))
                ++matchCount;
        }

        if (matchCount != gridNumbers.Count)
            return;

        // 一つの数字が完了していたら
        int index = number - 1;
        numberButton[index].image.raycastTarget = false;
        numberButton[index].image.color = completeNumberColor;
        foreach (var grid in gridNumbers)
        {
            grid.HighlightNumber(number, NumberGrid.ColorIndex.ClearHighlight);
        }
    }

    private void CheckNumbers(int number)
    {
        int matchCount = 0;
        foreach (var grid in gridNumbers)
        {
            if (grid.MatchNumber(number, null))
                ++matchCount;
        }

        if (matchCount != gridNumbers.Count)
            return;

        // 一つの数字が完了していたら
        int index = number - 1;
        numberButton[index].image.raycastTarget = false;
        numberButton[index].image.color = completeNumberColor;
        foreach (var grid in gridNumbers)
        {
            grid.HighlightNumber(number, NumberGrid.ColorIndex.ClearHighlight);
        }
    }

}