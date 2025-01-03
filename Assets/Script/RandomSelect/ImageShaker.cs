using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public partial class RandomSelecterView
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
        private bool snapping = false;
        [SerializeField] 
        private bool isHorizontal = false;

        private Vector2 initialPosition;
        private Tween shakeTween;

        public void Initialize()
        {
            initialPosition = targetImage.rectTransform.anchoredPosition;
        }

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
        public bool IsShaking()
        {
            return shakeTween != null;
        }

    }

}
