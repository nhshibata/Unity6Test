using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UseCountWindow : MonoBehaviour
{
    [SerializeField, Tooltip("スピンボタンのPrefab")]
    private GameObject spinButtonPrefab = null;
    [SerializeField, Tooltip("スピンボタンを配置するGridLayout")]
    private GridLayoutGroup gridLayout = null;
    [SerializeField, Tooltip("モーダル全体のルートオブジェクト")]
    private GameObject modalRoot = null;
    [SerializeField, Tooltip("閉じるボタン")]
    private Button closeButton = null;

    [SerializeField, Tooltip("1行あたりのボタン数")]
    private int buttonsPerRow = 5;

    private List<GameObject> spinButtons = new List<GameObject>();
    private bool isOpen = false;

    /// <summary>
    /// モーダルを初期化して開く
    /// </summary>
    /// <param name="min">最小値</param>
    /// <param name="max">最大値</param>
    /// <param name="onValueChange">値が変更されたときのコールバック</param>
    public void OpenModal(int min, int max, System.Action<int> onValueChange)
    {
        if (isOpen) return;

        UpdateGridLayout(min, max);
        GenerateSpinButtons(min, max, onValueChange);

        modalRoot.SetActive(true);
        isOpen = true;
    }

    /// <summary>
    /// モーダルを閉じる
    /// </summary>
    public void CloseModal()
    {
        if (!isOpen) return;

        modalRoot.SetActive(false);
        isOpen = false;
    }

    private void Start()
    {
        // 閉じるボタンのクリックイベント登録
        closeButton.onClick.AddListener(CloseModal);
    }

    private void UpdateGridLayout(int min, int max)
    {
        // グリッドの列数を設定
        int totalButtons = max - min + 1;
        int columns = Mathf.Min(buttonsPerRow, totalButtons);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    private void GenerateSpinButtons(int min, int max, System.Action<int> onValueChange)
    {
        // 必要なスピンボタン数
        int requiredButtons = max - min + 1;

        // ボタンを生成または再利用
        for (int i = 0; i < requiredButtons; i++)
        {
            GameObject button;
            if (i < spinButtons.Count)
            {
                button = spinButtons[i];
                button.SetActive(true);
            }
            else
            {
                button = Instantiate(spinButtonPrefab, gridLayout.transform);
                spinButtons.Add(button);
            }

            // スピンボタンの設定
            var spinButton = button.GetComponent<SpinButton>();
            int index = i + min;
            spinButton.AllRemoveListener();
            spinButton.AddListener(
                () => onValueChange(index + 1), // 増加時
                () => onValueChange(index - 1)  // 減少時
            );
        }

        // 余分なボタンを非表示
        for (int i = requiredButtons; i < spinButtons.Count; i++)
        {
            spinButtons[i].SetActive(false);
        }
    }
}
