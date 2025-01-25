using DG.Tweening;
using UnityEngine;

public abstract class RectTransformAnimator : MonoBehaviour
{
    [SerializeField]
    protected RectTransform targetRectTransform;
    [SerializeField]
    protected float animationDuration = 0.2f;

    protected Tween animationTween;


    protected virtual void Awake()
    {
        Initialize();
    }

    public abstract void Initialize();

    public abstract void StartAnimation();

    public virtual void StopAnimation()
    {
        if (animationTween != null && animationTween.IsActive())
        {
            animationTween.Kill();
            animationTween = null;
        }
    }

    /// <summary>
    /// アニメーション中かどうかを判定
    /// </summary>
    public bool IsAnimating()
    {
        return animationTween != null && animationTween.IsActive();
    }
}

