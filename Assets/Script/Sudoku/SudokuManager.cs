using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using static SudokuConfig;

public class SudokuManager : MonoBehaviour
{
    [SerializeField]
    private SudokuModel model;

    [SerializeField]
    private SudokuView view = null;


    private void Awake()
    {
        // ボタン押下で再生成
        view.GenerateButton.onClick.AddListener(() =>
        {
            ReStart();
        });

        // NumberToggleの状態変更を監視
        view.NumberToggle.OnValueChangedAsObservable()
            .Subscribe(value => Debug.Log($"Memo state => {value}"))
            .AddTo(this);

        // セルに入力する数字を選択
        view.SetNumberAction(value =>
        {
            model.SelectNumber.Value = value;
        });

        // セルが押された際の処理
        view.Initialize((x, y, grid) =>
        {
            bool isCorrect = model.CheckNumber(x, y);
            int select = model.SelectNumber.Value;
            if (!view.NumberToggle.isOn)
            {
                if (isCorrect)
                {
                    grid.SetNumber(NumberGrid.PosToIndex(x, y), select);
                    model.SetNumber(x, y, select);
                    view.StartSuccessEffect(select);
                    // 文言はviewに任せるべき？
                    view.SetMessage($"{select} is the correct answer!", true);
                }
                else
                {
                    view.SetMessage($"{select} is incorrect...", true);
                }
            }
            else
            {
                grid.SetCandidateNumber(NumberGrid.PosToIndex(x, y), select);
            }
        });

        view.Dropdown.onValueChanged.AddListener((value) => {
            model.Difficulty.Value = (DifficultyLevel)value;
        });

        // 完了通知を監視
        model.OnComplete
            .Subscribe(_ =>
            {
                view.SetMessage("Complete!", false);
            })
            .AddTo(this);

        model.Timer.
            Skip(1).
            Subscribe(time => 
            {
                view.SetTimerText(time);
            })
            .AddTo(this);
    }

    private void Start()
    {
        ReStart();
    }

    private void Update()
    {
        model.TimerUpdate();
    }

    /// <summary>
    /// 数独を再生成
    /// </summary>
    private void ReStart()
    {
        view.ReStart();
        model.Generate();
        view.SetGridData(model.HideGrid.CurrentValue);
    }

}
