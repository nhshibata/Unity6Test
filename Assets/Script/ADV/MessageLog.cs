using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageLog : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;
    [SerializeField]
    private TextMeshProUGUI logText = null;
    [SerializeField]
    private Button closeButton = null;

    private List<string> log = new List<string>();

    private void Awake()
    {
        Hide();
    }

    public void AddToLog(string characterName, string dialogue)
    {
        log.Add($"<b>{characterName}</b>: {dialogue}");
        UpdateLogDisplay();
    }

    private void UpdateLogDisplay()
    {
        logText.text = string.Join("\n", log);
    }

    public void ClearLog()
    {
        logText.text = string.Empty;
        log.Clear();
    }

    public void Show()
    {
        canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0.0f;
    }

}
