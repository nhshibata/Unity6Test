using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class HeartbeatEffect
{
    [SerializeField]
    private RectTransform heartUI;          
    [SerializeField]
    private float minScale = 1f;            
    [SerializeField]
    private float maxScale = 1.5f;          
    [SerializeField]
    private float minPulseSpeed = 1f;       
    [SerializeField]
    private float maxPulseSpeed = 0.5f;     
    [SerializeField]
    private float minPulseStrength = 0.1f;  
    [SerializeField]
    private float maxPulseStrength = 0.3f;  

    private Tween tween = null;

    // HPの割合をセットする関数（0〜1の間で渡す）
    public void SetHealthPercentage(float healthPercentage)
    {
        // HPの割合に基づいてスケールやテンポを調整
        float scaledPulseSpeed = Mathf.Lerp(maxPulseSpeed, minPulseSpeed, healthPercentage);
        float scaledPulseStrength = Mathf.Lerp(minPulseStrength, maxPulseStrength, healthPercentage);
        float scaledMaxScale = Mathf.Lerp(minScale, maxScale, healthPercentage);

        // DOTweenを使ってスケールのアニメーションを設定
        AnimateHeartbeat(scaledMaxScale, scaledPulseSpeed, scaledPulseStrength);
    }

    // 心臓の鼓動をアニメーションさせる関数
    private void AnimateHeartbeat(float targetScale, float pulseSpeed, float pulseStrength)
    {
        // 既存のアニメーションを停止（必要に応じて）
        tween.Kill();

        // DOTweenでスケールのアニメーションを作成
        tween = heartUI.DOScale(targetScale, pulseSpeed)
            .SetEase(Ease.InOutSine)        // ふわっとしたイージング
            .SetLoops(-1, LoopType.Yoyo)    // 永久ループで膨らんだり縮んだり
            .OnKill(() => heartUI.localScale = Vector3.one);  // アニメーションが終わったら元に戻す
    }
}
