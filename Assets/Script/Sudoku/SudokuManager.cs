using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using static SudokuConfig;

/// <summary>
/// 数独管理クラス
/// </summary>
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

        // ヒント
        view.OnHintClick += () => {
            var grid = model.GetHintGenerate();
            view.SetAllCandidateNumber(grid);
        };

        // セルが押された際の処理を追加
        view.Initialize((x, y, gridView) =>
        {
            bool isCorrect = model.CheckNumber(x, y);
            int select = model.SelectNumber.Value;

            // 数字記入か候補記入か
            if (!view.NumberToggle.isOn)
            {
                if (isCorrect)
                {
                    // 文言はviewに任せるべき？
                    view.SetMessage($"{select} is the correct answer!", true);

                    model.SetNumber(x, y, select);
                    gridView.SetNumber(NumberGrid.PosToIndex(x, y), select);
                    view.SetAllCandidateNumber(model.PossibleGrid.Value);
                    view.StartSuccessEffect(select);
                }
                else
                {
                    view.SetMessage($"{select} is incorrect...", true);
                }
            }
            else
            {
                gridView.SetCandidateNumber(NumberGrid.PosToIndex(x, y), select);
                model.UpdatePossibleGrid(x, y, select);
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

        // TODO: 自動で数字を埋めていくテストを追加
        // TODO: リザルトとなるウィンドウをポップアップ
        // TODO: 余裕があれば操作を設置
    }

    /// <summary>
    /// 数独を再生成
    /// </summary>
    private void ReStart()
    {
        view.ReStart();
        model.Generate();
        view.SetGridData(model.HideGrid.CurrentValue);
        view.SetAllCandidateNumber(model.PossibleGrid.Value);
    }

}
