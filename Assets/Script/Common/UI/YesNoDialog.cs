using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class YesNoDialog : BaseDialog
{
    /// <summary>
    /// Yes/Noダイアログを表示
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="onButtonPressed">押されたボタンのインデックスを返すコールバック</param>
    public void ShowYesNo(string message, System.Action<int> onButtonPressed)
    {
        var buttonLabels = new List<string> { "Yes", "No" };
        ShowDialog(message, buttonLabels, onButtonPressed).Forget();
    }
}