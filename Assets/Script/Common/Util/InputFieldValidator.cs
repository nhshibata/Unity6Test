using UnityEngine;

public static class InputFieldValidator
{
    /// <summary>
    /// 数字のみを許可する検証ロジック
    /// </summary>
    /// <param name="text">現在の入力テキスト</param>
    /// <param name="addChar">新しく追加された文字</param>
    /// <returns>許可される文字または '\0'</returns>
    public static char ValidateDigitOnly(string text, char addChar)
    {
        const int ASCII_LIMIT = 127;
        if (!char.IsDigit(addChar) || addChar > ASCII_LIMIT)
            return '\0';
        return addChar;
    }

    /// <summary>
    /// 入力の終了時に値を範囲内に収める処理
    /// </summary>
    /// <param name="input">現在の入力テキスト</param>
    /// <param name="min">許可される最小値</param>
    /// <param name="max">許可される最大値</param>
    /// <returns>修正された文字列</returns>
    public static string ClampInputToRange(string input, int min, int max)
    {
        if (string.IsNullOrEmpty(input))
            return min.ToString();

        if (int.TryParse(input, out int result))
            return Mathf.Clamp(result, min, max).ToString();

        return max.ToString();
    }

    /// <summary>
    /// 英数字のみを許可する検証
    /// </summary>
    /// <param name="text">現在の入力テキスト</param>
    /// <param name="addChar"></param>
    /// <returns>許可される文字または '\0'</returns>
    public static char ValidateAlphaNumeric(string text, char addChar)
    {
        if (!char.IsLetterOrDigit(addChar))
            return '\0';
        return addChar;
    }

    /// <summary>
    /// 小数点を含む数字の入力を許可する
    /// </summary>
    /// <param name="text">現在の入力テキスト</param>
    /// <param name="addChar">追加される文字</param>
    /// <returns>許可される文字または '\0'</returns>
    public static char ValidateDecimal(string text, char addChar)
    {
        if (!char.IsDigit(addChar) && addChar != '.')
            return '\0';
        if (addChar == '.' && text.Contains("."))
            return '\0';
        return addChar;
    }

}
