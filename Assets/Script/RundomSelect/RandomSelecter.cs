using System;
using System.Collections;
using UnityEngine;

public class RandomSelecter : MonoBehaviour
{
    [SerializeField]
    private RandomSelecterModel model;
    [SerializeField]
    private RandomSelecterView view;


    private void Awake()
    {
        model.LoadSettings();
        model.Init();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Func<string, char, char> onValidate = (text, addChar) =>
        {
            const int ASCII = 127;
            // 数字以外の場合は無視
            if (!char.IsDigit(addChar) || addChar > ASCII)
                return '\0';

            return addChar;
        };

        Func<string, int, int, string> onEndEdit = ((input, min, max) =>
        {
            // 入力欄が空の場合、最低値を設定する
            if (string.IsNullOrEmpty(input))
            {
                return min.ToString();
            }

            // 入力が数値かどうか確認、範囲内であればそのまま返す
            if (int.TryParse(input, out int result))
            {
                return input;
            }

            return max.ToString();
        });

        view.AddMaxListener(
            () => { model.SetMaxNumber(model.MaxNumber + 1); },
            () => { model.SetMaxNumber(model.MaxNumber - 1); },
            () => { return model.MaxNumber.ToString(); },
            (str, index, addChar) =>
            {
                char validatedChar = onValidate(str, addChar);
                if (validatedChar != '\0')
                {
                    // 数値としての有効な入力のみを処理
                    var newValue = int.Parse(str + validatedChar); // 入力文字列を結合
                    model.SetMaxNumber(newValue); // Modelに設定
                    return validatedChar; // 入力を適用
                }
                return '\0'; // 無効な入力は無視
            },
             (str) =>
             {
                 var adjustedValue = onEndEdit(str, RandomSelecterModel.RANGE_MIN, RandomSelecterModel.RANGE_MAX);
                 model.SetMaxNumber(int.Parse(adjustedValue)); // 入力値をModelに適用
                 return adjustedValue; // 確定値を返す
             }
            );

        view.AddMinListener(
            () => { model.SetMinNumber(model.MinNumber + 1); },
            () => { model.SetMinNumber(model.MinNumber - 1); },
            () => { return model.MinNumber.ToString(); },
            (str, index, addChar) =>
            {
                char validatedChar = onValidate(str, addChar);
                if (validatedChar != '\0')
                {
                    // 数値としての有効な入力のみを処理
                    var newValue = int.Parse(str + validatedChar); // 入力文字列を結合
                    model.SetMinNumber(newValue); // Modelに設定
                    return validatedChar; // 入力を適用
                }
                return '\0'; // 無効な入力は無視
            },
            (str) =>
            {
                var adjustedValue = onEndEdit(str, RandomSelecterModel.RANGE_MIN, RandomSelecterModel.RANGE_MAX);
                model.SetMinNumber(int.Parse(adjustedValue)); // 入力値をModelに適用
                return adjustedValue; // 確定値を返す
            }
            );

        view.AddToggleListener((value) =>
        {
            model.ShouldConsume = value;
        });

        view.OnClickStart += ((value) =>
        {
            if (value)
            {
                Debug.Log($"Mapサイズ: {model.GetMapSize()}");
                Debug.Log($"使用可能な数字がある: {model.HasUsableNumbers()}");

                if (model.HasUsableNumbers())
                {
                    StopCoroutine("SelectNumber");
                    StartCoroutine("SelectNumber");
                }
                else
                {
                    Debug.LogWarning("使用可能な数字がありません。コルーチンは開始されません。");
                }
            }
            else
            {
                // コルーチンが実行中であれば停止
                StopCoroutine("SelectNumber");
                model.ConfirmedNumber();
                view.SetRandomNumberText(model.SelectNumber);
            }
        });

    }

    private void OnEnable()
    {
        model.Init();
    }

    private void OnDisable()
    {
        model.SaveSettings();
    }

    private IEnumerator SelectNumber()
    {
        while (true)
        {
            int select = model.GetRandomNumber();
            if (select == RandomSelecterModel.EXIT_NUMBER)
                break;

            model.SelectNumber = select;
            view.SetRandomNumberText(select);
            yield return null;
        }

        Debug.Log("想定外な終了");
    }

}
