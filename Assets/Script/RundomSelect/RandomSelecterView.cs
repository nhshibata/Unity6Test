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
        [SerializeField] 
        private Image targetImage;
        [SerializeField] 
        private float shakeDuration = 1f;
        [SerializeField] 
        private float shakeStrength = 10f;
        [SerializeField] 
        private int vibrato = 10; // 振動回数
        [SerializeField] 
        private bool snapping = false;
        [SerializeField] 
        private bool isHorizontal = false;

        private Vector2 initialPosition;
        private Tween shakeTween;

        public void StartShake()
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
        /// 画像を横方向に振動させる
        /// </summary>
        public void ShakeHorizontal()
        {
            initialPosition = targetImage.rectTransform.anchoredPosition; // 初期位置を保存
            StopShake(); // 既存の振動を停止

            shakeTween = targetImage.rectTransform.DOAnchorPosX(
                initialPosition.x + shakeStrength,
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
            initialPosition = targetImage.rectTransform.anchoredPosition; // 初期位置を保存
            StopShake(); // 既存の振動を停止

            shakeTween = targetImage.rectTransform.DOAnchorPosY(
                initialPosition.y + shakeStrength,
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
                shakeTween.Kill();
                shakeTween = null;
            }

            // 初期位置に戻す
            targetImage.rectTransform.DOAnchorPos(initialPosition, 0.5f).SetEase(Ease.OutQuad); // 緩やかに戻す
        }

        /// <summary>
        /// 画像のスプライトを変更
        /// </summary>
        public void ChangeSprite(Sprite sprite)
        {
            targetImage.sprite = sprite;
        }

        /// <summary>
        /// 振動中かどうかを判定
        /// </summary>
        public bool IsShake()
        {
            return shakeTween != null;
        }

    }

    [SerializeField]
    private SpinButtonController minSpinButton;
    [SerializeField]
    private SpinButtonController maxSpinButton;
    [SerializeField]
    private Toggle toggle = null;
    [SerializeField]
    private ButtonBinder startButton = null;
    [SerializeField]
    private TMP_Text numberText = null;
    [SerializeField]
    private ImageShaker shaker = null;
    [SerializeField] 
    private Image targetImage;
    [SerializeField] 
    private float rotationDuration = 50f;
    [SerializeField]
    private List<Sprite> charSprites = new List<Sprite>();

    public Action<bool> OnClickStart {  get; set; }

    private Tween rotateTween = null;


    private void Start()
    {
        startButton.Button.onClick.AddListener(OnClickStartAnimation);
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {       
            shaker.StopShake();
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Kill();
            }
        }
    }

    public void AddMinListener(Action increase, Action decrease, Func<string> displayUpdate = null,
        TMPro.TMP_InputField.OnValidateInput onValidateInput = null, Func<string, string> onEndEdit = null)
    {
        minSpinButton.AddListener(increase, decrease, displayUpdate, onValidateInput, onEndEdit);
    }
    
    public void AddMaxListener(Action increase, Action decrease, Func<string> displayUpdate = null,
        TMPro.TMP_InputField.OnValidateInput onValidateInput = null, Func<string, string> onEndEdit = null)
    {
        maxSpinButton.AddListener(increase, decrease, displayUpdate, onValidateInput, onEndEdit);
    }

    public void AddToggleListener(Action<bool> action)
    {
        toggle.onValueChanged.AddListener((value) => { 
            action?.Invoke(value);
        });
    }

    public void OnClickStartAnimation()
    {
        // スタートが押される前
        if (shaker.IsShake())
        {
            shaker.ChangeSprite(charSprites[0]);
            shaker.StopShake();
            OnClickStart(false);
            rotateTween.Kill();

            startButton.Text.text = "Start";
            Debug.Log("ランダムストップ");
        }
        else
        {
            int index = UnityEngine.Random.Range(0, charSprites.Count);
            shaker.ChangeSprite(charSprites[index]);
            shaker.StartShake();
            OnClickStart(true);
            rotateTween = targetImage.rectTransform.DORotate(
                new Vector3(0, 0, 360),
                rotationDuration,
                RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart); // 無限ループ

            startButton.Text.text = "Stop";
            Debug.Log("ランダムスタート");
        }
    }

    public void SetRandomNumberText(int value)
    {
        numberText.text = value.ToString();
    }

}
