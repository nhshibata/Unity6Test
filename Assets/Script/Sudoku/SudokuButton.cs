using TMPro;
using UnityEngine;

/// <summary>
/// 数独用のボタン
/// 元のButtonと表示Textに加えて候補の数字用のTextを追加
/// </summary>
public class SudokuButton : ButtonBinder
{
    [SerializeField]
    private TMP_Text candidateText = null;

    private int candidateBits = 0;


    private void Awake()
    {
        ClearText();
    }

    /// <summary>
    /// 候補数字を設定する
    /// </summary>
    /// <param name="number">追加または削除する数字 (1~9)</param>
    public void SetCandidateNumber(int number)
    {
        if (number < 1 || number > 9)
            return;

        int bit = 1 << (number - 1);
        if ((candidateBits & bit) != 0)
        {
            candidateBits &= ~bit;
        }
        else
        {
            candidateBits |= bit;
        }

        UpdateCandidateText();
    }

    public void UpdateCandidateNumber(int bits)
    {
        candidateBits = bits;
        UpdateCandidateText();
    }

    /// <summary>
    /// 現在の候補数字をテキストに反映する
    /// </summary>
    private void UpdateCandidateText()
    {
        string number = string.Empty;
        for (int i = 1; i <= 9; i++)
        {
            if ((candidateBits & (1 << (i - 1))) != 0)
            {
                number += i;
            }
            else
            {
                // 記入されていない場合は空白
                number += "  ";
            }

            // 改行する
            if (i % 3 == 0)
                number += "\n";
        }
        candidateText.text = number;
    }

    public void SetCandidateEnable(bool enable)
    {
        candidateText.gameObject.SetActive(enable);
    }

    public void ClearText()
    {
        candidateText.text = string.Empty;
    }
}