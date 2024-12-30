using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.IO;
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

    private List<int> queuedSelections = new List<int>();
    private ObservableList<int> selectionLimits = new ObservableList<int>();
    private Dictionary<int, int> selectionCountMap = new Dictionary<int, int>();

    private ModalSettings currentSettings;

    private IDisposable selectNumberSubscription; 


    public void Init()
    {
        LoadSettings().Forget();
        queuedSelections.Clear();

        if (selectionLimits.Count < maxNumber.Value)
        {
            for (int i = selectionLimits.Count; i < maxNumber.Value; i++)
            {
                selectionLimits.Add(1);
            }
        }

        Debug.Log("リストサイズ" + selectionLimits.Count);
        for (int i = minNumber.Value; i < maxNumber.Value; i++)
        {
            if (selectionCountMap.ContainsKey(i))
            {
                selectionCountMap[i] = selectionLimits[i];
            }
            else
            {
                selectionCountMap.Add(i, selectionLimits[i]);
            }
        }

        // 既存の購読を解除してから新しい購読を登録
        selectNumberSubscription?.Dispose();
        selectNumberSubscription = prevNumber.Skip(1).Subscribe(value => queuedSelections.Add(value));
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
    /// 使用可能な数字があるかどうかを確認する関数
    /// </summary>
    public bool HasUsableNumbers()
    {
        if (!shouldConsume.Value)
            return true;

        foreach (var kvp in selectionCountMap)
        {
            if (kvp.Value > 0)
                return true;
        }

        return false;
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
        if (index - 1 < 0 || index - 1 >= selectionLimits.Count)
        {
            Debug.LogError($"無効なインデックス: {index}");
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
        Debug.Log($"{prevNumber.Value} current{selectNumber.Value}");
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
            Debug.LogError("設定できない:max");
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
            Debug.LogError("設定できない:min");
            return;
        }

        minNumber.Value = value;

        if (maxNumber.Value < minNumber.Value + 1)
        {
            maxNumber.Value = minNumber.Value + 1;
        }

        AdjustListSize();
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

        string json = JsonUtility.ToJson(currentSettings, true);
        await File.WriteAllTextAsync(filePath, json);

        Debug.Log($"Settings saved to: {filePath}");
    }

    private void AdjustListSize()
    {
        // list のサイズを maxNumber に合わせて拡張
        while (selectionLimits.Count < maxNumber.Value)
        {
            selectionLimits.Add(1); // デフォルト値（例：1）で埋める
        }

        // list のサイズを minNumber に合わせて縮小
        while (selectionLimits.Count > maxNumber.Value)
        {
            selectionLimits.RemoveAt(selectionLimits.Count - 1); // 不要な要素を削除
        }

        // map を再設定（list に合わせる）
        for (int i = minNumber.Value; i < maxNumber.Value; i++)
        {
            if (selectionCountMap.ContainsKey(i))
            {
                selectionCountMap[i] = selectionLimits[i];
            }
            else
            {
                selectionCountMap.Add(i, selectionLimits[i]);
            }
        }

        Debug.Log("リストのサイズを調整しました: " + selectionLimits.Count);
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

        Debug.Log($"Loaded Settings: Min={currentSettings.MinValue}, Max={currentSettings.MaxValue}, List={string.Join(", ", currentSettings.AvailableNumbers)}");
    }

}
