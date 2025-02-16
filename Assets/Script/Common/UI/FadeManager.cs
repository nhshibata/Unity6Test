using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float defaultFadeDuration = 0.5f;

    /// <summary>
    /// フェードイン処理
    /// </summary>
    /// <param name="duration">フェード時間（省略時はデフォルト値）</param>
    /// <param name="onComplete">完了時のコールバック</param>
    public async UniTask FadeInAsync(float? duration = null, Action onComplete = null)
    {
        float fadeDuration = duration ?? defaultFadeDuration;
        await Fade(0f, 1f, fadeDuration);
        onComplete?.Invoke();
    }

    /// <summary>
    /// フェードアウト処理
    /// </summary>
    /// <param name="duration">フェード時間（省略時はデフォルト値）</param>
    /// <param name="onComplete">完了時のコールバック</param>
    public async UniTask FadeOutAsync(float? duration = null, Action onComplete = null)
    {
        float fadeDuration = duration ?? defaultFadeDuration;
        await Fade(1f, 0f, fadeDuration);
        onComplete?.Invoke();
    }

    /// <summary>
    /// フェード処理（内部共通ロジック）
    /// </summary>
    /// <param name="from">開始透明度</param>
    /// <param name="to">終了透明度</param>
    /// <param name="duration">フェード時間</param>
    private async UniTask Fade(float from, float to, float duration)
    {
        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = true; // フェード中はUIの操作をブロック
        Debug.Log($"fade実行");

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / duration);
            await UniTask.Yield();
        }

        canvasGroup.alpha = to;

        // 完全に透明ならUI操作を許可しない
        canvasGroup.blocksRaycasts = to > 0f;
    }
}
