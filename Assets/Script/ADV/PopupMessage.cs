using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessage : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform = null;

    [SerializeField]
    private Image backgroundImage = null;

    [SerializeField]
    private TextMeshProUGUI messageText = null;

    [SerializeField]
    private float displayDuration = 2.0f;

    [SerializeField]
    private float animationDuration = 0.5f;


    private void Awake()
    {
        SetAlpha(0);
    }

    /// <summary>
    /// ポップアップメッセージを表示
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <returns></returns>
    public async UniTask ShowMessage(string message)
    {
        messageText.text = message;

        // フェードインアニメーション
        await AnimatePopup(true);

        // 指定時間表示
        await UniTask.Delay(TimeSpan.FromSeconds(displayDuration));

        // フェードアウトアニメーション
        await AnimatePopup(false);
    }

    /// <summary>
    /// ポップアップのアニメーション処理
    /// </summary>
    /// <param name="isShowing">true: 表示, false: 非表示</param>
    private async UniTask AnimatePopup(bool isShowing)
    {
        float elapsedTime = 0f;
        float startAlpha = isShowing ? 0f : 1f;
        float endAlpha = isShowing ? 1f : 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / animationDuration);
            SetAlpha(alpha);

            // ポップアップのスケールもアニメーションさせる
            float scale = Mathf.Lerp(isShowing ? 0.8f : 1f, isShowing ? 1f : 0.8f, elapsedTime / animationDuration);
            rectTransform.localScale = new Vector3(scale, scale, 1f);

            await UniTask.Yield();
        }

        SetAlpha(endAlpha);
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    /// <summary>
    /// アルファ値の設定
    /// </summary>
    /// <param name="alpha">設定するアルファ値</param>
    private void SetAlpha(float alpha)
    {
        Color bgColor = backgroundImage.color;
        backgroundImage.color = new Color(bgColor.r, bgColor.g, bgColor.b, alpha);

        Color textColor = messageText.color;
        messageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
    }

}
