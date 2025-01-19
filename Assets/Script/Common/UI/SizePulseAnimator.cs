using DG.Tweening;
using System;
using UnityEngine;

public class SizePulseAnimator : RectTransformAnimator
{
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
        if (targetRectTransform != null)
        {
            initialSize = targetRectTransform.sizeDelta;
        }
    }

    /// <summary>
    /// アニメーションを開始する
    /// </summary>
    public override void StartAnimation()
    {
        if (targetRectTransform == null) return;

        StopAnimation(); // 既存のアニメーションを停止

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

        // サイズを一瞬縮小して戻すループアニメーション
        animationTween = targetRectTransform.DOSizeDelta(targetSize, animationDuration / 2)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo); // Yoyoモーションで戻す
    }

    /// <summary>
    /// アニメーションを停止し、サイズを初期値に戻す
    /// </summary>
    public override void StopAnimation()
    {
        base.StopAnimation();

        // 初期サイズに戻す
        if (targetRectTransform != null)
        {
            targetRectTransform.DOSizeDelta(initialSize, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}
