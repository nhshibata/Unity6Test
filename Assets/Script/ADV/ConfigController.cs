using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;

[Serializable]
public class ConfigData
{
    public float bgmVolume = 1.0f;
    public float seVolume = 1.0f;
    public bool autoMode = false;
}

#region Controller
public class ConfigController : MonoBehaviour
{
    #region Model
    public class ConfigModel
    {
        private const string SaveKey = "ADV_Config";
        private ReactiveProperty<ConfigData> configData;

        public ReadOnlyReactiveProperty<ConfigData> ConfigData => configData;

        public ConfigModel()
        {
            Load();
        }

        public void UpdateConfig(Action<ConfigData> updateAction)
        {
            updateAction?.Invoke(configData.Value);
            configData.OnNext(configData.Value); // 強制通知
            Save();
        }

        private void Save()
        {
            string json = JsonUtility.ToJson(configData.Value);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                configData = new ReactiveProperty<ConfigData>(JsonUtility.FromJson<ConfigData>(json));
            }
            else
            {
                configData = new ReactiveProperty<ConfigData>(new ConfigData());
            }
        }
    }
    #endregion

    [SerializeField]
    private ConfigView view = null;

    private ConfigModel model;


    private void Start()
    {
        // モデルのデータをUIに反映
        model.ConfigData.Subscribe(config => {
            view.SetConfig(config);
        }).AddTo(this);

        // UIの変更をモデルに反映
        view.OnBgmVolumeChanged.Subscribe(volume => {
            model.UpdateConfig(config => config.bgmVolume = volume);
        }).AddTo(this);

        view.OnSeVolumeChanged.Subscribe(volume => {
            model.UpdateConfig(config => config.seVolume = volume);
        }).AddTo(this);

        view.OnAutoModeChanged.Subscribe(isOn => {
            model.UpdateConfig(config => config.autoMode = isOn);
        }).AddTo(this);

        // 閉じるボタン
        view.OnCloseButtonPressed.Subscribe(_ => {
            CloseConfigUIAsync().Forget();
        }).AddTo(this);
    }

    private async UniTaskVoid CloseConfigUIAsync()
    {
        await UniTask.Delay(300);
        view.Hide();
    }
}
#endregion
