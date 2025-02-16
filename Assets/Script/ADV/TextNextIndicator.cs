using DG.Tweening;
using UnityEngine;

public class TextNextIndicator
{
    private Tween iconAnimation = null;

    /// <summary>
    /// アニメーションの開始
    /// </summary>
    public void StartTapIconAnimation(RectTransform textAdvanceIcon)
    {
        // アニメーションが既に存在している場合はリセット
        if (iconAnimation != null && iconAnimation.IsActive())
        {
            iconAnimation.Kill();
        }

        // アルファ値のフェードアニメーション（例）
        CanvasGroup canvasGroup = textAdvanceIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textAdvanceIcon.gameObject.AddComponent<CanvasGroup>();
        }

        iconAnimation = canvasGroup.DOFade(0.2f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // または上下に動かすアニメーション（例）
        // tapAnimationTween = tapWaitUI.DOAnchorPosY(tapWaitUI.anchoredPosition.y + 10, 0.5f)
        //     .SetLoops(-1, LoopType.Yoyo)
        //     .SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// アニメーションの停止
    /// </summary>
    public void StopTapIconAnimation(RectTransform textAdvanceIcon)
    {
        if (iconAnimation != null && iconAnimation.IsActive())
        {
            iconAnimation.Kill();
        }

        // アルファ値をリセット
        CanvasGroup canvasGroup = textAdvanceIcon.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // 元の位置に戻す場合（上下移動の場合）
        // tapWaitUI.anchoredPosition = new Vector2(tapWaitUI.anchoredPosition.x, 初期位置);
    }
}
