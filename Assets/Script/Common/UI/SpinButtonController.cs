using System;
using UnityEngine;

/// <summary>
/// スピンボタン操作時にTMP_InputFieldへ反映する
/// TMP_InputField入力時の制限は外部から設定する
/// </summary>
public class SpinButtonController : SpinButton
{
    [SerializeField, Tooltip("値を表示")]
    private TMPro.TMP_InputField displayInput = null;

    public Func<string> displayUpdateCallback = null;


    /// <summary>
    /// コールバック追加
    /// </summary>
    /// <param name="increase">増加</param>
    /// <param name="decrease">減少</param>
    /// <param name="displayUpdate">TMPへ反映させたい文</param>
    /// <param name="onValidateInput">TMP_InputFieldへの入力時制限</param>
    public void AddListener(Action increase, Action decrease, Func<string> displayUpdate = null, Action<string> onTextChanged = null,
        TMPro.TMP_InputField.OnValidateInput onValidateInput = null, Func<string, string> onEndEdit = null)
    {
        if (displayUpdate != null)
        {
            displayUpdateCallback += displayUpdate;
            displayInput.text = displayUpdateCallback();
        }

        if (onValidateInput != null)
        {
            displayInput.onValidateInput += onValidateInput;
        }

        if (onEndEdit != null)
        {
            displayInput.onEndEdit.AddListener((x) =>
            {
                displayInput.text = onEndEdit(x);
            });
        }

        if (onTextChanged != null)
        {
            displayInput.onValueChanged.AddListener((newText) =>
            {
                onTextChanged(newText);
                if (displayUpdateCallback != null)
                {
                    displayInput.text = displayUpdateCallback();
                }
            });
        }

        onIncreaseAction += () =>
        {
            increase();
            if (displayUpdateCallback != null)
            {
                displayInput.text = displayUpdateCallback();
            }
        };

        onDecreaseAction += () =>
        {
            decrease();
            if (displayUpdateCallback != null)
            {
                displayInput.text = displayUpdateCallback();
            }
        };

        increaseButton.onClick.AddListener(onIncreaseAction);
        decreaseButton.onClick.AddListener(onDecreaseAction);
    }

    /// <summary>
    /// 直接Textを変更
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        displayInput.text = text;
    }

}