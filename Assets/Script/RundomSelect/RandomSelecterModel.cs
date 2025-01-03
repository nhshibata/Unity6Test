using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class RandomSelecterModel
{
    /// <summary>
    /// 保存するデータ
    /// </summary>
    [System.Serializable]
    public class ModalSettings
    {
        public int MinValue;
        public int MaxValue;
        public bool ShouldConsume = false;
        public List<int> AvailableNumbers;
    }

    public const int RANGE_MIN = 0;
    public const int RANGE_MAX = 999;
    public const int EXIT_NUMBER = -1;
    private const string FileName = "ModalSettings.json";

    [SerializeField, Tooltip("デフォルトの最小値")]
    private int defaultMin = 1;
    [SerializeField, Tooltip("デフォルトの最大値")]
    private int defaultMax = 10;

    private ReactiveProperty<int> maxNumber = new ReactiveProperty<int>(10);
    private ReactiveProperty<int> minNumber = new ReactiveProperty<int>(1);
    private ReactiveProperty<int> prevNumber = new ReactiveProperty<int>();
    private ReactiveProperty<int> selectNumber = new ReactiveProperty<int>(0);
    private ReactiveProperty<bool> shouldConsume = new ReactiveProperty<bool>(true);

    public ReadOnlyReactiveProperty<int> MaxNumber => maxNumber;
    public ReadOnlyReactiveProperty<int> MinNumber => minNumber;
    public ReadOnlyReactiveProperty<int> PrevNumber => prevNumber;
    public ReactiveProperty<int> SelectNumber => selectNumber;
    public ReactiveProperty<bool> ShouldConsume => shouldConsume;

    public Action OnLoadComplete { get; set; }

    private List<int> queuedSelections = new List<int>();
    private ObservableList<int> selectionLimits = new ObservableList<int>();
    private Dictionary<int, int> selectionCountMap = new Dictionary<int, int>();

    private ModalSettings currentSettings;

    private IDisposable selectNumberSubscription; 


    public void Init()
    {
        OnLoadComplete += () =>
        {
            if (selectionLimits.Count < maxNumber.Value)
            {
                for (int i = selectionLimits.Count; i < maxNumber.Value; i++)
                {
                    selectionLimits.Add(1);
                }
            }

            Debug.Log("リストサイズ" + selectionLimits.Count);
            //for (int i = minNumber.Value; i <= maxNumber.Value; i++)
            //{
            //    if (selectionCountMap.ContainsKey(i))
            //    {
            //        selectionCountMap[i] = selectionLimits[i];
            //    }
            //    else
            //    {
            //        selectionCountMap.Add(i, selectionLimits[i]);
            //    }
            //}
        };

        LoadSettings().Forget();
        queuedSelections.Clear();

        // 既存の購読を解除してから新しい購読を登録
        selectNumberSubscription?.Dispose();
        selectNumberSubscription = prevNumber.Skip(1).Subscribe(value => { queuedSelections.Add(value); });
    }

    public void Reset()
    {
        Init();
    }

    /// <summary>
    /// minとmaxの差分からmapのサイズを取得する関数
    /// </summary>
    public int GetMapSize()
    {
        return maxNumber.Value - minNumber.Value + 1;
    }

    /// <summary>
    /// 使用可能な数字のカウントを返す関数
    /// </summary>
    public int GetUsableNumbersCount()
    {
        int usableCount = 0;
        foreach (var kvp in selectionCountMap)
        {
            if (kvp.Value > 0)  // 数字が使える場合のみカウント
                usableCount++;
        }
        Debug.Log($"{selectionCountMap.First().Key}:{selectionCountMap.Last().Key}");
        return usableCount;
    }

    /// <summary>
    /// 使用可能な数字があるかどうかを確認する関数
    /// </summary>
    public bool HasUsableNumbers()
    {
        if (!shouldConsume.Value)
            return true;

        return GetUsableNumbersCount() > 0;
    }

    public async UniTask<int> GetRandomNumberAsync()
    {
        if (!HasUsableNumbers())
        {
            Debug.LogError("使用可能な数字がありません");
            return EXIT_NUMBER;
        }

        const int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int select = UnityEngine.Random.Range(minNumber.Value, maxNumber.Value + 1);

            if (IsSelect(select))
            {
                selectNumber.Value = select;
                return select;
            }

            attempts++;
            await UniTask.Yield();
        }

        return EXIT_NUMBER;
    }

    public bool IsSelect(int index)
    {
        if (index < 0 || index > selectionLimits.Count)
        {
            Debug.LogWarning($"無効なインデックス: {index}");
            return false;
        }

        if (selectionCountMap.ContainsKey(index))
        {
            var num = selectionCountMap[index];
            return (num > 0);
        }
        else
        {
            int reg = selectionLimits[index - 1];
            selectionCountMap.Add(index, reg);
            Debug.Log($"mapに存在しないため、{index}に{reg}を設定");
        }

        return true;
    }

    public void ConfirmedNumber()
    {
        Debug.Log($"表示する値を確定 : {prevNumber.Value} current{selectNumber.Value}");
        prevNumber.Value = selectNumber.Value;

        if (shouldConsume.Value)
        {
            selectionCountMap[selectNumber.Value]--;
        }
    }

    public void SetMaxNumber(int value)
    {
        if (value < minNumber.Value + 1 || value > RANGE_MAX)
        {
            Debug.LogWarning("設定できない:max");
            return;
        }

        maxNumber.Value = value;

        if (minNumber.Value > maxNumber.Value - 1)
        {
            minNumber.Value = maxNumber.Value - 1;
        }

        AdjustListSize();
    }

    public void SetMinNumber(int value)
    {
        if (value < RANGE_MIN || value > maxNumber.Value - 1)
        {
            Debug.LogWarning("設定できない:min");
            return;
        }

        minNumber.Value = value;

        if (maxNumber.Value < minNumber.Value + 1)
        {
            maxNumber.Value = minNumber.Value + 1;
        }

        AdjustListSize();
    }

    private void AdjustListSize()
    {
        // selectionLimitsのサイズをmaxNumberに合わせて調整
        while (selectionLimits.Count < maxNumber.Value + 1)
        {
            selectionLimits.Add(1); // デフォルト値（例：1）で埋める
        }

        // selectionLimitsのサイズをmaxNumberに合わせて縮小
        while (selectionLimits.Count > maxNumber.Value + 1)
        {
            selectionLimits.RemoveAt(selectionLimits.Count - 1); // 不要な要素を削除
        }

        // selectionCountMapをselectionLimitsに合わせて再設定
        for (int i = minNumber.Value; i < maxNumber.Value + 1; i++) // maxNumber を含めるために <= を使用
        {
            if (selectionCountMap.ContainsKey(i))
            {
                selectionCountMap[i] = selectionLimits[i - minNumber.Value]; // 修正: インデックス調整
            }
            else
            {
                selectionCountMap.Add(i, selectionLimits[i - minNumber.Value]);
            }
        }

        Debug.Log("リストとマップのサイズを調整しました: " + selectionLimits.Count);
    }

    public List<int> GetSelectionsInOrder(bool fifo, bool ascendingOrder)
    {
        Debug.Log($"{queuedSelections.Count}キューのサイズ");
        
        if (fifo)
        {
            return queuedSelections;
        }
        else
        {
            var orderedList = new List<int>(queuedSelections);
            if (ascendingOrder)
            {
                orderedList.Sort(); // 昇順でソート
            }
            else
            {
                orderedList.Sort((a, b) => b.CompareTo(a)); // 降順でソート
            }
            return orderedList;
        }
    }

    public Dictionary<int, int> GetSelectionCount()
    {
        Dictionary<int, int> ret = new Dictionary<int, int>(selectionCountMap);
        return ret;
    }

    public async UniTask SaveSettingsAsync()
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileName);

        // 現在の設定を反映
        currentSettings = new ModalSettings
        {
            MinValue = minNumber.Value,
            MaxValue = maxNumber.Value,
            ShouldConsume = shouldConsume.Value,
            AvailableNumbers = new List<int>(this.selectionLimits)
        }; 

        string json = JsonUtility.ToJson(currentSettings, true);
        await File.WriteAllTextAsync(filePath, json);

        Debug.Log($"Settings saved to: {filePath}");
    }

    public async UniTask LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileName);

        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            currentSettings = JsonUtility.FromJson<ModalSettings>(json);
        }
        else
        {
            currentSettings = new ModalSettings
            {
                MinValue = defaultMin,
                MaxValue = defaultMax,
                ShouldConsume = shouldConsume.Value,
                AvailableNumbers = new List<int>()
            };

            for (int i = defaultMin; i <= defaultMax; i++)
            {
                currentSettings.AvailableNumbers.Add(i);
            }
        }

        selectionLimits = new ObservableList<int>();
        SetMinNumber(currentSettings.MinValue);
        SetMaxNumber(currentSettings.MaxValue);
        shouldConsume.Value = currentSettings.ShouldConsume;

        OnLoadComplete?.Invoke();
        OnLoadComplete = null;
        Debug.Log($"Loaded Settings :{filePath}: Min={currentSettings.MinValue}, Max={currentSettings.MaxValue}, List={string.Join(", ", currentSettings.AvailableNumbers)}");
    }

}
