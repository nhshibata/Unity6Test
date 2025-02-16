using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static ADVCsvReader;

[Serializable]
public class ADVModel
{
    [Serializable]
    public class PageData
    {
        public int ScenarioNo;
        public int ScenarioOrder;
    }

    private static readonly string SaveKey = "ADV_Save";

    private ReactiveProperty<bool> isAuto = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> IsAuto { get => isAuto; set => isAuto = value; }
    
    private ReactiveProperty<float> autoDelay = new ReactiveProperty<float>(2.0f);
    public ReactiveProperty<float> AutoDelay { get => autoDelay; set => autoDelay = value; }

    private ADVCsvReader csvReader = null;

    private ReactiveProperty<PageData> currentPage = new ReactiveProperty<PageData>(new PageData());
    private ReactiveProperty<PageData> nextPage = new ReactiveProperty<PageData>(new PageData());

    private CancellationTokenSource cancellationTokenSource;

    public Action<UIReflection> OnPageUpdated { get; set; }

    // TODO:仮の物
    public TextAsset layersCsv = null;
    public TextAsset characterCsv = null;
    public TextAsset textureCsv = null;
    public TextAsset scenarioLabelCsv = null;
    public TextAsset scenarioCsv = null;


    public void Init()
    {
        nextPage.Value.ScenarioNo = 1;
        nextPage.Value.ScenarioOrder = 1;

        // データを読み込む
        csvReader = new ADVCsvReader(layersCsv, characterCsv, textureCsv, scenarioLabelCsv, scenarioCsv);
        var reflec = csvReader.PrintScenarioDetails(nextPage.Value.ScenarioNo, nextPage.Value.ScenarioOrder);
        currentPage = nextPage;
        nextPage.Value.ScenarioNo = reflec.NextNo;
        nextPage.Value.ScenarioOrder = reflec.NextOrder;
    }

    /// <summary>
    /// 自動モードを開始
    /// </summary>
    public async void StartAutoMode()
    {
        if (isAuto.Value)
        {
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (isAuto.Value)
                {
                    await Task.Delay(TimeSpan.FromSeconds(autoDelay.Value), cancellationTokenSource.Token);
                    UpdatePage();
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }

    /// <summary>
    /// 自動モードを停止する
    /// </summary>
    public void StopAutoMode()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// ページを更新する
    /// </summary>
    public void UpdatePage(PageData page = null)
    {
        var next = page ?? nextPage.Value;
        var reflec = csvReader.PrintScenarioDetails(next.ScenarioNo, next.ScenarioOrder);

        // 次のシナリオ情報を更新
        currentPage.Value = next;
        nextPage.Value.ScenarioNo = reflec.NextNo;
        nextPage.Value.ScenarioOrder = reflec.NextOrder;

        // コールバックを呼び出す
        OnPageUpdated?.Invoke(reflec);
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(currentPage.Value);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log($"currentPage {currentPage.Value.ScenarioNo}-{currentPage.Value.ScenarioOrder}");
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            PageData loadedData = JsonUtility.FromJson<PageData>(json);
            currentPage.Value = loadedData;  // 値を直接設定
            Debug.Log($"currentPage {loadedData.ScenarioNo}-{loadedData.ScenarioOrder}");
        }
        else
        {
            // PlayerPrefsにデータがない場合は初期化
            currentPage.Value = new PageData { ScenarioNo = 1, ScenarioOrder = 1 };
        }
        Debug.Log($"currentPage {currentPage.Value.ScenarioNo}-{currentPage.Value.ScenarioOrder}");

        // 現在のページ情報を取得
        UpdatePage(currentPage.Value);
    }


}

