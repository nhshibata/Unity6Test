using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomSelecterView : MonoBehaviour
{
    [Serializable]
    public class ImageShaker
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private float shakeDuration = 1f; // 振動の継続時間
        [SerializeField] private float shakeStrength = 10f; // 振動の強度
        [SerializeField] private int vibrato = 10; // 振動回数
        [SerializeField] private bool snapping = false; // スナップするか
        private Tween shakeTween;

        /// <summary>
        /// 画像を横方向に振動させる
        /// </summary>
        public void ShakeHorizontal()
        {
            StopShake(); // 既存の振動を停止
            shakeTween = targetImage.rectTransform.DOAnchorPosX(
                targetImage.rectTransform.anchoredPosition.x + shakeStrength,
                shakeDuration,
                snapping)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo); // 無限ループでYoyoモーション
        }

        /// <summary>
        /// 画像を縦方向に振動させる
        /// </summary>
        public void ShakeVertical()
        {
            StopShake(); // 既存の振動を停止
            shakeTween = targetImage.rectTransform.DOAnchorPosY(
                targetImage.rectTransform.anchoredPosition.y + shakeStrength,
                shakeDuration,
                snapping)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo); // 無限ループでYoyoモーション
        }

        /// <summary>
        /// 振動を停止する
        /// </summary>
        public void StopShake()
        {
            if (shakeTween != null && shakeTween.IsActive())
            {
                shakeTween.Kill(); // 振動を即停止
                shakeTween = null;
            }
        }

        public void ChangeSprite(Sprite sprite)
        {
            targetImage.sprite = sprite;
        }

        public bool IsShake()
        {
            return shakeTween != null;
        }

    }

    [SerializeField]
    private InputField minInput;
    [SerializeField]
    private InputField maxInput;
    [SerializeField]
    private Toggle toggle = null;
    [SerializeField]
    private Button startButton = null;
    [SerializeField]
    private TMP_Text numberText = null;
    [SerializeField]
    private ImageShaker shaker = null;
    [SerializeField]
    private List<Sprite> charSprites = new List<Sprite>();

    public Action<bool> OnClickStart {  get; set; }


    private void Start()
    {
        startButton.onClick.AddListener(OnClickStartAnimation);
    }

    private void OnDisable()
    {
        shaker.StopShake();
    }

    public void AddMinListener(Action<int> listener)
    {
        minInput.onValueChanged.AddListener((value) =>
        {
            if(int.TryParse(value, out int val))
            {
                listener?.Invoke(val);
            }
        });
    }
    
    public void AddMaxListener(Action<int> listener)
    {
        maxInput.onValueChanged.AddListener((value) =>
        {
            if(int.TryParse(value, out int val))
            {
                listener?.Invoke(val);
            }
        });
    }

    public void AddToggleListener(Action<bool> action)
    {
        toggle.onValueChanged.AddListener((value) => { 
            action?.Invoke(value);
        });
    }

    public void OnClickStartAnimation()
    {
        if (shaker.IsShake())
        {
            int index = UnityEngine.Random.Range(0, charSprites.Count);
            shaker.ShakeHorizontal();
            shaker.ChangeSprite(charSprites[index]);
            OnClickStart(true);
        }
        else
        {
            shaker.StopShake();
            OnClickStart(false);
        }
    }

    public void SetNumber(int value)
    {
        numberText.text = value.ToString();
    }

}
