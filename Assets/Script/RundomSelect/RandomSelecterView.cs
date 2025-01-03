using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class RandomSelecterView : MonoBehaviour
{
    private const int DEFAULT_FACE = 0;

    [SerializeField]
    private SpinButtonController minSpinButton;
    [SerializeField]
    private SpinButtonController maxSpinButton;
    [SerializeField]
    private Toggle toggle = null;
    [SerializeField]
    private ButtonBinder startButton = null;
    [SerializeField]
    private Button resetButton = null;
    [SerializeField]
    private TMP_Text numberText = null;
    [SerializeField]
    private ImageShaker shaker = null;
    [SerializeField] 
    private RectTransform target = null;
    [SerializeField] 
    private float rotationDuration = 50.0f;
    [SerializeField]
    private SelectionDisplayManager selectionDisplayManager = null;
    [SerializeField]
    private List<Sprite> charSprites = new List<Sprite>();

    public Action<bool> OnClickStart {  get; set; }

    private Tween rotateTween = null;


    private void Awake()
    {
        startButton.Button.onClick.AddListener(ToggleAnimation);
        shaker.Initialize();
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            shaker.StopShake();
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Kill();
                rotateTween = null;
            }
        }

        selectionDisplayManager.Reset();
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

    public void AddResetButtonListener(Action action)
    {
        resetButton.onClick.AddListener(() => { action?.Invoke(); });
    }

    public void SetHistoryNumbers(Func<bool, bool, List<int>> ret, Func<Dictionary<int, int>> selectCount)
    {
        selectionDisplayManager.SetHistoryNumbers(ret, selectCount);
    }

    private void ToggleAnimation()
    {
        if (shaker.IsShaking())
        {
            StopRandomAnimation();
        }
        else
        {
            StartRandomAnimation();
        }
    }

    private void StartRandomAnimation()
    {
        int index = UnityEngine.Random.Range(DEFAULT_FACE + 1, charSprites.Count);
        shaker.ChangeSprite(charSprites[index]);
        shaker.StartShake();
        OnClickStart?.Invoke(true);

        rotateTween = target.DORotate(new Vector3(0, 0, 360), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        startButton.Text.text = "Stop";
    }

    private void StopRandomAnimation()
    {
        shaker.ChangeSprite(charSprites[DEFAULT_FACE]);
        shaker.StopShake();
        OnClickStart?.Invoke(false);
        rotateTween?.Kill();
        rotateTween = null;

        startButton.Text.text = "Start";
    }

    public void SetMinText(int min)
    {
        minSpinButton.SetText(min.ToString());
    }

    public void SetMaxText(int max)
    {
        maxSpinButton.SetText(max.ToString());
    }

    public void SetRandomNumberText(int value)
    {
        numberText.text = value.ToString();
    }

    public void AddPrevNumber(int value)
    {
        selectionDisplayManager.SetHistory(value);
    }

}
