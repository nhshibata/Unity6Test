using DG.Tweening;
using UnityEngine;

public class SizePulseAnimator : RectTransformAnimator
{
    [SerializeField]
    private float delay = 0.1f;
    [SerializeField]
    private float sizeMultiplier = 0.8f; // 縮小倍率
    [SerializeField]
    private bool affectX = true; // X方向に影響するか
    [SerializeField]
    private bool affectY = true; // Y方向に影響するか

    private Vector2 initialSize;


    /// <summary>
    /// 初期化する（初期サイズを保存）
    /// </summary>
    public override void Initialize()
    {
        initialSize = targetRectTransform.sizeDelta;
    }

    /// <summary>
    /// アニメーションを開始する
    /// </summary>
    public override void StartAnimation()
    {
        StopAnimation();

        // アニメーションターゲットのサイズ設定
        Vector2 targetSize = initialSize;
        if (affectX)
        {
            targetSize.x *= sizeMultiplier;
        }
        if (affectY)
        {
            targetSize.y *= sizeMultiplier;
        }

        // サイズを一瞬縮小して戻すアニメーション
        animationTween = targetRectTransform.DOSizeDelta(targetSize, animationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // サイズ変更が完了したら元のサイズに戻す
                targetRectTransform.DOSizeDelta(initialSize, animationDuration / 2)
                    .SetDelay(delay)
                    .SetEase(Ease.InSine);
            });
    }

    /// <summary>
    /// アニメーションを停止し、サイズを初期値に戻す
    /// </summary>
    public override void StopAnimation()
    {
        base.StopAnimation();

        // 初期サイズに戻す
        Vector2 size = ((initialSize) == Vector2.zero ? targetRectTransform.sizeDelta : initialSize);
        targetRectTransform.DOSizeDelta(size, animationDuration).SetEase(Ease.OutQuad);
    }


    [SerializeField]
    private bool isTest = false;
    private void Update()
    {
        if (!isTest)
            return;

        isTest = false;
        StartAnimation();
    }

}
