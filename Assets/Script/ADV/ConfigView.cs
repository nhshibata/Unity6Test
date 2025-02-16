using R3;
using UnityEngine;
using UnityEngine.UI;

public class ConfigView : MonoBehaviour
{
    [SerializeField]
    private Slider bgmVolumeSlider = null;
    [SerializeField]
    private Slider seVolumeSlider = null;
    [SerializeField]
    private Toggle autoModeToggle = null;
    [SerializeField]
    private Button closeButton = null;
    [SerializeField]
    private Image blockPanel = null;
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    public Observable<float> OnBgmVolumeChanged => bgmVolumeSlider.OnValueChangedAsObservable();
    public Observable<float> OnSeVolumeChanged => seVolumeSlider.OnValueChangedAsObservable();
    public Observable<bool> OnAutoModeChanged => autoModeToggle.OnValueChangedAsObservable();
    public Observable<Unit> OnCloseButtonPressed => closeButton.OnClickAsObservable();


    private void Awake()
    {
        Hide();
    }

    public void SetConfig(ConfigData config)
    {
        bgmVolumeSlider.value = config.bgmVolume;
        seVolumeSlider.value = config.seVolume;
        autoModeToggle.isOn = config.autoMode;
    }

    public void Show()
    {
        blockPanel.raycastTarget = true;
        canvasGroup.alpha = 1.0f;
    }

    public void Hide()
    {
        blockPanel.raycastTarget = false;
        canvasGroup.alpha = 0.0f;
    }
}
