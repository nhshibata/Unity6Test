using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSpriteManager : MonoBehaviour
{
    private const int DEFAULT_FACE = 0;

    [SerializeField]
    private Image image = null;
    [SerializeField]
    private RectTransformAnimator rectAnim;
    public RectTransformAnimator RectAnim { get => rectAnim; }
    [SerializeField]
    private List<Sprite> charSprites;

    [SerializeField]
    private float fadeDuration = 0.5f;


    public void Awake()
    {
        ChangeSprite(charSprites[DEFAULT_FACE]);
    }

    public void ChangeToRandomCharacter()
    {
        int index = UnityEngine.Random.Range(DEFAULT_FACE + 1, charSprites.Count);
        ChangeSprite(charSprites[index]);
    }

    public void ResetToDefaultCharacter()
    {
        ChangeSprite(charSprites[DEFAULT_FACE]);
    }

    public void StartShaking()
    {
        rectAnim.StartAnimation();
    }

    public void StopShaking()
    {
        rectAnim.StopAnimation();
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
        image.DOColor(Color.black, fadeDuration).OnComplete(()=>onComplete?.Invoke());
    }
    
    public void StartWhiteFade(Action<Image> onStart, Action onComplete)
    {
        onStart?.Invoke(image);
        image.DOColor(Color.white, fadeDuration).OnComplete(()=>onComplete?.Invoke());
    }

}
