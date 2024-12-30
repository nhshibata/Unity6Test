using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

public class RandomSelecter : MonoBehaviour
{
    [SerializeField]
    private RandomSelecterModel model;
    [SerializeField]
    private RandomSelecterView view;

    private readonly CompositeDisposable disposables = new CompositeDisposable();
    private CancellationTokenSource cts;


    private void Awake()
    {
        Func<string, char, char> onValidate = (text, addChar) =>
        {
            const int ASCII = 127;
            if (!char.IsDigit(addChar) || addChar > ASCII)
                return '\0';
            return addChar;
        };

        Func<string, int, int, string> onEndEdit = (input, min, max) =>
        {
            if (string.IsNullOrEmpty(input))
                return min.ToString();

            if (int.TryParse(input, out int result))
                return Mathf.Clamp(result, min, max).ToString();

            return max.ToString();
        };

        model.Init();

        //=============================
        // イベント登録
        //=============================
        view.AddMaxListener(
          () => model.SetMaxNumber(model.MaxNumber.CurrentValue + 1),
          () => model.SetMaxNumber(model.MaxNumber.CurrentValue - 1),
          () => model.MaxNumber.CurrentValue.ToString(),
          (str, index, addChar) => InputFieldValidator.ValidateDigitOnly(str, addChar),
          str => InputFieldValidator.ClampInputToRange(str, RandomSelecterModel.RANGE_MIN, RandomSelecterModel.RANGE_MAX)
        );

        view.AddMinListener(
            () => model.SetMinNumber(model.MinNumber.CurrentValue + 1),
            () => model.SetMinNumber(model.MinNumber.CurrentValue - 1),
            () => model.MinNumber.CurrentValue.ToString(),
            (str, index, addChar) => InputFieldValidator.ValidateDigitOnly(str, addChar),
            str => InputFieldValidator.ClampInputToRange(str, RandomSelecterModel.RANGE_MIN, RandomSelecterModel.RANGE_MAX)
        );

        view.AddToggleListener(value => model.ShouldConsume.Value = value);
        view.AddResetButtonListener(() => { model.Reset(); });
        view.SetHistoryNumbers((fifo, order) => model.GetSelectionsInOrder(fifo, order));

        view.OnClickStart += async (value) =>
        {
            if (value)
            {
                if (model.HasUsableNumbers())
                {
                    cts = new CancellationTokenSource(); // 新しいトークンを作成
                    try
                    {
                        await SelectNumberAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // キャンセルされた場合の処理
                        Debug.Log($"SelectNumberAsync が安全にキャンセル{model.SelectNumber.CurrentValue}で終了");
                        view.SetRandomNumberText(model.SelectNumber.CurrentValue);
                        model.ConfirmedNumber();
                    }
                    catch (Exception ex)
                    {
                        // 他の予期しないエラーを処理
                        Debug.LogError($"予期しないエラー: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("使用可能な数字がありません");
                }
            }
            else
            {
                cts?.Cancel();
                
            }
        };


        // ReactiveProperty の監視
        model.SelectNumber
            .Skip(1)
            .Subscribe(current =>
            {
                // 必要に応じて処理
                view.SetRandomNumberText(current);
            })
            .AddTo(disposables);

        model.PrevNumber
            .Skip(1)
            .Pairwise()
            .Subscribe(pair =>
            {
                Debug.Log($"前の値: {pair.Previous}, 現在の値: {pair.Current}");
                view.AddPrevNumber(pair.Previous);
            })
            .AddTo(disposables);
    }

    private void OnEnable()
    {
        model.Init();
    }

    private async void OnDisable()
    {
        try
        {
            await model.SaveSettingsAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSettingsAsync failed: {e}");
        }
        disposables.Clear();
    }

    private async UniTask SelectNumberAsync(CancellationToken token)
    {
        // UniTask.Yield によって非同期タスクがキャンセルされるタイミングでチェックする
        while (!token.IsCancellationRequested)
        {
            int select = await model.GetRandomNumberAsync();
            if (select == RandomSelecterModel.EXIT_NUMBER)
                break;

            model.SelectNumber.Value = select;
            Debug.Log($"終了していない");

            // キャンセルのタイミングを確保
            await UniTask.Yield(token);
        }
    }

}
