using UnityEngine;

public class SudokuManager : MonoBehaviour
{
    [SerializeField]
    private SudokuModel model;

    [SerializeField]
    private SudokuView view = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 作成
        view.GenerateButton.onClick.AddListener(() => {
            ReStart();
        });

        view.NumberToggle.onValueChanged.AddListener((value) => { Debug.Log($"memo state =>{value}"); });

        // セルに入力する数字を選択
        view.SetNumberAction((value) => { 
            model.SelectNumber = value;
        });

        // セルが押された際に現在選択中の数字を反映する
        // 正解であれば通常の数字が入り、失敗であれば赤色の数字が入る
        view.Initialize((x, y, grid) => {
            model.CheckNumber(x, y);

            if (!view.NumberToggle.isOn)
            {
                // 正解であれば反映
                if (model.CheckNumber(x, y))
                {
                    grid.SetNumber(NumberGrid.PosToIndex(x, y), model.SelectNumber);

                    view.SetMessage($"{model.SelectNumber} is correct answer");
                }
                else
                {
                    // 不正解であればメッセージを表示
                    view.ResetGridColors();
                    view.SetMessage($"{model.SelectNumber} is incorrect answer");
                }
            }
            else
            {
                grid.SetCandidateNumber(NumberGrid.PosToIndex(x, y), model.SelectNumber);
            }
        });

        model.OnComplete += () => { view.SetMessage($"Complete!"); };
    }

    private void ReStart()
    {
        view.ReStart();
        model.Generate();
        view.SetGridData(model.HideGrid);
    }

}
