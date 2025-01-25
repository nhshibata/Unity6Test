using DG.Tweening;
using UnityEngine;

public class ImageShaker : RectTransformAnimator
{
    [SerializeField]
    private float shakeStrength = 20.0f;
    [SerializeField]
    private bool snapping = false;
    [SerializeField]
    private bool isHorizontal = false;

    private Vector2 initialPosition;


    public override void Initialize()
    {
        initialPosition = targetRectTransform.anchoredPosition;
    }

    public override void StartAnimation()
    {
        if (isHorizontal)
        {
            ShakeHorizontal();
        }
        else
        {
            ShakeVertical();
        }
    }

    /// <summary>
    /// 振動を停止する
    /// </summary>
    public override void StopAnimation()
    {
        base.StopAnimation();

        // 初期位置に戻す
        targetRectTransform.DOAnchorPos(initialPosition, 0.5f).SetEase(Ease.OutQuad); // 緩やかに戻す
    }

    /// <summary>
    /// 画像を横方向に振動させる
    /// </summary>
    public void ShakeHorizontal()
    {
        initialPosition = targetRectTransform.anchoredPosition; // 初期位置を保存
        StopAnimation(); 

        animationTween = targetRectTransform.DOAnchorPosX(
            initialPosition.x + shakeStrength,
            animationDuration,
            snapping)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo); // 無限ループでYoyoモーション
    }

    /// <summary>
    /// 画像を縦方向に振動させる
    /// </summary>
    public void ShakeVertical()
    {
        initialPosition = targetRectTransform.anchoredPosition; // 初期位置を保存
        StopAnimation();

        animationTween = targetRectTransform.DOAnchorPosY(
            initialPosition.y + shakeStrength,
            animationDuration,
            snapping)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo); // 無限ループでYoyoモーション
    }

}
