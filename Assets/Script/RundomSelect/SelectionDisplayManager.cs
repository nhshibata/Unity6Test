using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionDisplayManager : MonoBehaviour
{
    private enum DropdownSelect
    {
        FIFO,
        Ascending,
        Descending,
    }

    [Header("UI References")]
    [SerializeField]
    private TextBinding selectionPanelPrefab = null;
    [SerializeField]
    private Button openButton = null;
    [SerializeField]
    private Button closeButton = null;
    [SerializeField]
    private Transform contentTransform = null;
    [SerializeField]
    private TMP_Dropdown orderDropdown = null;
    [SerializeField]
    private RectTransform historyPanel = null;
    [SerializeField]
    private List<TMP_Text> historyNumber = new List<TMP_Text>();

    private List<int> currentSelections = new List<int>();
    private List<TextBinding> selectionPanels = new List<TextBinding>();
    private Func<bool, bool, List<int>> GetSelection { get; set; }


    private void Awake()
    {
        historyPanel.gameObject.SetActive(false);

        // DropdownSelect 列挙体をループして文字列リストに追加
        List<string> options = new List<string>();
        foreach (DropdownSelect option in Enum.GetValues(typeof(DropdownSelect)))
        {
            options.Add(option.ToString());
        }

        // ドロップダウンの選択肢を設定
        orderDropdown.ClearOptions();
        orderDropdown.AddOptions(options);

        // ドロップダウンの選択変更に応じて処理を実行
        orderDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        openButton.onClick.AddListener(() => { historyPanel.gameObject.SetActive(true); orderDropdown.onValueChanged.Invoke(orderDropdown.value); });
        closeButton.onClick.AddListener(() => { historyPanel.gameObject.SetActive(false); });
    }

    public void Reset()
    {
        // 表示を[-]で初期化
        historyNumber.ForEach(history => { history.text = "-"; });
    }

    public void SetHistoryNumbers(Func<bool, bool, List<int>> ret)
    {
        GetSelection = ret;
    }

    public void SetHistory(int number)
    {
        for (int i = historyNumber.Count - 1; 0 < i; --i)
        {
            historyNumber[i].text = historyNumber[i - 1].text;
        }

        historyNumber[0].text = number.ToString();
    }

    private List<int> GetSelectionsInOrder(bool fifo, bool ascendingOrder)
    {
        List<int> orderedList = new List<int>(currentSelections);

        if (!fifo)
        {
            if (ascendingOrder)
            {
                orderedList.Sort();
            }
            else
            {
                orderedList.Sort((a, b) => b.CompareTo(a));
            }
        }
        return orderedList;
    }

    private void CreateOrUpdateSelectionPanel(int selection)
    {
        // すでに生成されたパネルがあるか確認
        TextBinding panel = selectionPanels.Find(p => !p.gameObject.activeSelf);

        if (panel == null)
        {
            // 新しくパネルを生成
            panel = Instantiate(selectionPanelPrefab, contentTransform);
            selectionPanels.Add(panel); // 生成したパネルをリストに追加
        }

        // パネルに選択肢を設定
        panel.Text.text = selection.ToString();
        panel.gameObject.SetActive(true);
    }

    private void ResetUI()
    {
        // 既存のパネルを非表示にする
        foreach (Transform child in contentTransform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void DisplaySelections(bool fifo, bool ascendingOrder)
    {
        var list = GetSelection.Invoke(fifo, ascendingOrder);
        currentSelections = list;

        ResetUI();

        // 選択肢を取得して表示
        List<int> selectionsToDisplay = currentSelections;
        int num = 0;
        foreach (var selection in selectionsToDisplay)
        {
            num++;
            CreateOrUpdateSelectionPanel(selection);
        }
        Debug.Log($"{num}");
    }

    private void OnDropdownValueChanged(int index)
    {
        DropdownSelect select = (DropdownSelect)index;
        Debug.Log($"{select}によって並び替え");

        switch (select)
        {
            case DropdownSelect.FIFO:
                // FIFOが選択された場合
                SetFifoOrder();
                break;
            case DropdownSelect.Ascending:
                // 昇順が選択された場合
                SetAscendingOrder();
                break;
            case DropdownSelect.Descending:
                // 降順が選択された場合
                SetDescendingOrder();
                break;
            default:
                break;
        }
    }

    private void SetFifoOrder()
    {
        DisplaySelections(true, false);
    }

    private void SetAscendingOrder()
    {
        DisplaySelections(false, true);
    }

    private void SetDescendingOrder()
    {
        DisplaySelections(false, false);
    }
}
