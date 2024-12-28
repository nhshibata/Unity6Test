using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private UIDocument uIDocument = null;

    private Tween alphaTween = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        var lt = root.Q<Label>("label_lt");
        var logo = root.Q<VisualElement>("logo");
        var tapToStartButton = root.Q<Button>();

        lt.text = "変更されている";
        tapToStartButton.clicked += () =>
        {
            Debug.Log("画面全体の透明アニメーション処理実行");

            var currentOpacity = logo.resolvedStyle.opacity;
            Alpha(logo, 1.0f, currentOpacity < 0.5f ? 1.0f : 0.0f);
        };

        // 2秒かけて(-100, 0, 0)に向かってtargetを移動させる
        alphaTween.Kill();
        DOTween.To(() => logo.transform.position,
            x => logo.transform.position = x, new Vector3(-100, 0), 2f)
            .SetEase(Ease.OutQuart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Alpha(VisualElement ve, float duration, float alphaValue)
    {
        var currentOpacity = ve.resolvedStyle.opacity;
        alphaTween = DOTween.To(() => currentOpacity,
                x => ve.style.opacity = new StyleFloat(x), alphaValue, duration);
    }
}
