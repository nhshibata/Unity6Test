using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseDialog : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI messageText = null;
    [SerializeField] 
    private Transform buttonContainer = null;  
    [SerializeField] 
    private ButtonBinder buttonPrefab = null;        
    [SerializeField] 
    private CanvasGroup canvasGroup = null;
    [SerializeField] 
    private float fadeDuration = 0.5f;
    
    private List<ButtonBinder> buttons = new List<ButtonBinder>();   


    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// ダイアログを表示する
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="buttonLabels">ボタンのラベルリスト</param>
    /// <param name="onButtonPressed">押されたボタンのインデックスを返すコールバック</param>
    public async UniTask ShowDialog(string message, List<string> buttonLabels, Action<int> onButtonPressed)
    {
        messageText.text = message;
        if(buttons.Count == 0)
        {
            CreateButtons(buttonLabels, onButtonPressed);
        }
        else
        {
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(true);
            }
        }

        // フェードイン
        await FadeCanvasGroup(true);
    }

    /// <summary>
    /// ダイアログを非表示にする
    /// </summary>
    public async UniTask HideDialog()
    {
        // フェードアウト
        await FadeCanvasGroup(false);

        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ボタンを動的に生成する
    /// </summary>
    private void CreateButtons(List<string> buttonLabels, Action<int> onButtonPressed)
    {
        for (int i = 0; i < buttonLabels.Count; i++)
        {
            int index = i; // クロージャキャプチャ防止
            var button = Instantiate(buttonPrefab, buttonContainer);
            button.Text.text = buttonLabels[i];
            button.Button.onClick.AddListener(() => {
                onButtonPressed?.Invoke(index);
                HideDialog().Forget();
            });

            buttons.Add(button);
        }
    }

    /// <summary>
    /// CanvasGroupのフェード処理
    /// </summary>
    private async UniTask FadeCanvasGroup(bool isVisible)
    {
        float elapsedTime = 0f;
        float startAlpha = isVisible ? 0f : 1f;
        float endAlpha = isVisible ? 1f : 0f;

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            await UniTask.Yield();
        }

        canvasGroup.alpha = endAlpha;
    }
}
