using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static CsvReader;

public class ADVModel
{
    [Serializable]
    public class PageData
    {
        private int scenarioNo = 1;
        public int ScenarioNo { get => scenarioNo; set => scenarioNo = value; }
        private int scenarioOrder = 1;
        public int ScenarioOrder { get => scenarioOrder; set => scenarioOrder = value; }

        public PageData(int scenarioNo, int scenarioOrder)
        {
            this.scenarioNo = scenarioNo;
            this.scenarioOrder = scenarioOrder;
        }
    }

    private static readonly string SaveKey = "ADV_Save";

    private ReactiveProperty<bool> isAuto = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> IsAuto { get => isAuto; set => isAuto = value; }
    
    private ReactiveProperty<float> autoDelay = new ReactiveProperty<float>(2.0f);
    public ReactiveProperty<float> AutoDelay { get => autoDelay; set => autoDelay = value; }

    private CsvReader csvReader = null;

    private ReactiveProperty<PageData> currentPage = new ReactiveProperty<PageData>(new PageData(1, 1));
    private ReactiveProperty<PageData> nextPage = new ReactiveProperty<PageData>(new PageData(1, 1));

    private CancellationTokenSource cancellationTokenSource;

    public Action<UIReflection> OnPageUpdated { get; set; }


    public void Init()
    {
        // データを読み込む
        TextAsset layersCsv = null;
        TextAsset characterCsv = null;
        TextAsset textureCsv = null;
        TextAsset scenarioLabelCsv = null;
        TextAsset scenarioCsv = null;

        csvReader = new CsvReader(layersCsv, characterCsv, textureCsv, scenarioLabelCsv, scenarioCsv);
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
        string json = JsonUtility.ToJson(currentPage);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            currentPage = new ReactiveProperty<PageData>(JsonUtility.FromJson<PageData>(json));
        }
        else
        {
            currentPage = new ReactiveProperty<PageData>(new PageData(1, 1));
        }

        // 現在のページ情報を取得
        UpdatePage(currentPage.Value);
    }

}

