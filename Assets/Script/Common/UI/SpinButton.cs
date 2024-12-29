using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 矢印による増減を行うスピンボタン
/// 条件が満たない場合は更新しない
/// </summary>
public class SpinButton : MonoBehaviour
{
    [SerializeField, Tooltip("+")]
    protected Button increaseButton = null;
    [SerializeField, Tooltip("-")]
    protected Button decreaseButton = null;
    [SerializeField]
    private EventTrigger increaseTrigger = null;
    [SerializeField]
    private EventTrigger decreaseTrigger = null;

    [SerializeField, Tooltip("加速が開始されるまでの時間")]
    private float accelerationThreshold = 1f;
    [SerializeField, Tooltip("加速後の繰り返し処理の間隔")]
    private float repeatRate = 0.1f;

    protected UnityAction onIncreaseAction;
    protected UnityAction onDecreaseAction;

    private bool isIncreaseButtonHeld = false;
    private bool isDecreaseButtonHeld = false;

    private float holdTime = 0f;


    private void Start()
    {
        // Down,Up時の動作追加
        EventTrigger.Entry increasePointerDownEntry = new EventTrigger.Entry();
        increasePointerDownEntry.eventID = EventTriggerType.PointerDown;
        increasePointerDownEntry.callback.AddListener(StartIncreaseHold);

        EventTrigger.Entry decreasePointerDownEntry = new EventTrigger.Entry();
        decreasePointerDownEntry.eventID = EventTriggerType.PointerDown;
        decreasePointerDownEntry.callback.AddListener(StartDecreaseHold);

        EventTrigger.Entry increasePointerUpEntry = new EventTrigger.Entry();
        increasePointerUpEntry.eventID = EventTriggerType.PointerUp;
        increasePointerUpEntry.callback.AddListener(StopIncreaseHold);

        EventTrigger.Entry decreasePointerUpEntry = new EventTrigger.Entry();
        decreasePointerUpEntry.eventID = EventTriggerType.PointerUp;
        decreasePointerUpEntry.callback.AddListener(StopDecreaseHold);

        increaseTrigger.triggers.Add(increasePointerDownEntry);
        decreaseTrigger.triggers.Add(decreasePointerDownEntry);
        increaseTrigger.triggers.Add(increasePointerUpEntry);
        decreaseTrigger.triggers.Add(decreasePointerUpEntry);
    }

    private void Update()
    {
        // 増加
        if (isIncreaseButtonHeld)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= accelerationThreshold)
            {
                if (holdTime % repeatRate < Time.deltaTime)
                {
                    onIncreaseAction?.Invoke();
                }
            }
        }

        // 減少
        if (isDecreaseButtonHeld)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= accelerationThreshold)
            {
                if (holdTime % repeatRate < Time.deltaTime)
                {
                    onDecreaseAction?.Invoke();
                }
            }
        }
    }

    public void AddListener(Action increase, Action decrease)
    {
        onIncreaseAction += () =>
        {
            increase();
        };

        onDecreaseAction += () =>
        {
            decrease();
        };

        increaseButton.onClick.AddListener(onIncreaseAction);
        decreaseButton.onClick.AddListener(onDecreaseAction);
    }

    public void RemoveListener()
    {
        increaseButton.onClick.RemoveListener(onIncreaseAction);
        decreaseButton.onClick.RemoveListener(onDecreaseAction);
    }

    public void AllRemoveListener()
    {
        increaseButton.onClick.RemoveAllListeners();
        decreaseButton.onClick.RemoveAllListeners();
    }

    protected void StartIncreaseHold(BaseEventData baseEvent)
    {
        isIncreaseButtonHeld = true;
        holdTime = 0f;
    }

    protected void StopIncreaseHold(BaseEventData baseEvent)
    {
        isIncreaseButtonHeld = false;
        holdTime = 0f;
    }

    protected void StartDecreaseHold(BaseEventData baseEvent)
    {
        isDecreaseButtonHeld = true;
        holdTime = 0f;
    }

    protected void StopDecreaseHold(BaseEventData baseEvent)
    {
        isDecreaseButtonHeld = false;
        holdTime = 0f;
    }
}
