using UnityEngine;

public class ADVController : MonoBehaviour
{
    [SerializeField]
    private ADVModel model;
    [SerializeField]
    private ADVView view = null;


    void Start()
    {
        model.OnPageUpdated += (reflection) => {
            view.DataToUpdate(reflection);
        };

        // メッセージウィンドウを押したら次の文を表示
        view.OnNextButtonPressed += () => {
            model.UpdatePage();
        };

        view.OnAutoToggleChanged += (value) => {
            model.IsAuto.Value = value;
            if (model.IsAuto.Value)
            {
                model.StartAutoMode();
            }
            else
            {
                model.StopAutoMode();
            }
        };

        view.OnSkipPressed += () => {

        };

        view.OnSavePressed += () => {

        };
        
        view.OnLoadPressed += () => {

        };
        
        view.OnQuickSavePressed += () => {
            model.Save();
        };
        
        view.OnQuickLoadPressed += () => {
            _ = view.FadeManager.FadeInAsync(null, null);
        };

        view.OnConfigPressed += () => {

        };

        view.OnLogButtonPressed -= () => {

        };

    }

    void Update()
    {
        
    }

    public void StopAuto()
    {
        // 自動モードを停止
        model.IsAuto.Value = false;
        model.StopAutoMode();
    }

    public void OnCancel()
    {
        
    }

}
