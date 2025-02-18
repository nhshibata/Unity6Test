using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSpriteManagerBase : MonoBehaviour
{
    [Header("Refarence")]
    [SerializeField]
    protected Image image = null;
    [SerializeField]
    protected RectTransformAnimator rectAnim;
    public RectTransformAnimator RectAnim { get => rectAnim; }

    [Header("Parameters")]
    [SerializeField]
    protected float fadeDuration = 0.5f;


    public virtual void ResetToDefaultCharacter()
    {

    }
    
    public virtual void ChangeToRandomCharacter()
    {

    }

    /// <summary>
    /// 画像のスプライトを変更
    /// </summary>
    public void ChangeSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void StartBlackFade(Action<Image> onStart, Action onComplete)
    {
        onStart?.Invoke(image);
        image.DOColor(Color.black, fadeDuration).OnComplete(() => onComplete?.Invoke());
    }

    public void StartWhiteFade(Action<Image> onStart, Action onComplete)
    {
        onStart?.Invoke(image);
        image.DOColor(Color.white, fadeDuration).OnComplete(() => onComplete?.Invoke());
    }
}
