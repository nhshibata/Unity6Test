using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RandomSelecterModel
{
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

    [SerializeField, Tooltip("デフォルトの最小値")]
    private int defaultMin = 1;
    [SerializeField, Tooltip("デフォルトの最大値")]
    private int defaultMax = 10;

    [SerializeField]
    private int maxNumber = 10;
    [SerializeField]
    private int minNumber = 1;

    [SerializeField]
    private bool shouldConsume = true;
    
    [SerializeField]
    private List<int> list = new List<int>();

    private int selectNumber = 0;
    private Dictionary<int, int> map = new Dictionary<int, int>();

    private const string FileName = "ModalSettings.json";
    private ModalSettings currentSettings;

    public int MaxNumber { get => maxNumber; set => maxNumber = value; }
    public int MinNumber { get => minNumber; set => minNumber = value; }
    public int SelectNumber { get => selectNumber; set => selectNumber = value; }
    public bool ShouldConsume { get => shouldConsume; set => shouldConsume = value; }


    public void Init()
    {
        LoadSettings();

        if (list.Count < maxNumber)
        {
            for (int i = list.Count; i < maxNumber; i++)
            {
                list.Add(1);
            }
        }

        Debug.Log("リストサイズ" + list.Count);
        for (int i = minNumber; i < maxNumber; i++)
        {
            if (map.ContainsKey(i))
            {
                map[i] = list[i];
            }
            else
            {
                map.Add(i, list[i]);
            }
        }
    }

    /// <summary>
    /// minとmaxの差分からmapのサイズを取得する関数
    /// </summary>
    public int GetMapSize()
    {
        return maxNumber - minNumber + 1;
    }

    /// <summary>
    /// 使用可能な数字があるかどうかを確認する関数
    /// </summary>
    public bool HasUsableNumbers()
    {
        // isUseがfalseの場合は消費しないのでtrueを返す
        if (!shouldConsume) 
            return true;

        int usableCount = 0;

        // map内の値を確認
        foreach (var kvp in map)
        {
            if (kvp.Value > 0) 
                usableCount++;
        }
        return usableCount > 0;
    }

    public int GetRandomNumber()
    {
        if (!HasUsableNumbers())
        {
            Debug.LogError("使用可能な数字がありません");
            return EXIT_NUMBER;
        }

        const int maxAttempts = 100; // 無限ループ防止用
        int attempts = 0;

        int select;

        while (attempts < maxAttempts)
        {
            select = UnityEngine.Random.Range(minNumber, maxNumber + 1);

            if (IsSelect(select))
            {
                return select;
            }

            attempts++;
        }

        return EXIT_NUMBER;
    }

    public bool IsSelect(int index)
    {

        if (index - 1 < 0 || index - 1 >= list.Count)
        {
            Debug.LogError($"無効なインデックス: {index}");
            return false;
        }

        if (map.ContainsKey(index))
        {
            var num = map[index];
            return (num > 0);
        }
        else
        {
            int reg = list[index - 1];
            map.Add(index, reg);
            Debug.Log($"mapに存在しないため、{index}に{reg}を設定");
        }

        return true;
    }

    public void ConfirmedNumber()
    {
        if (shouldConsume)
            map[selectNumber]--;
    }

    public void SetMaxNumber(int maxNumber)
    {
        if (maxNumber < minNumber + 1 || maxNumber > RANGE_MAX)
        {
            Debug.Log("設定できない:max");
            return;
        }

        this.maxNumber = maxNumber;

        if (this.minNumber > this.maxNumber - 1)
        {
            this.minNumber = this.maxNumber - 1;
        }
    }

    public void SetMinNumber(int minNumber)
    {
        if (minNumber < RANGE_MIN || minNumber > maxNumber - 1)
        {
            Debug.Log("設定できない:min");
            return;
        }

        this.minNumber = minNumber;

        if (this.maxNumber < this.minNumber + 1)
        {
            this.maxNumber = this.minNumber + 1;
        }
    }


    /// <summary>
    /// 設定データを保存
    /// </summary>
    public void SaveSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileName);

        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Settings saved to: {filePath}");
    }

    /// <summary>
    /// 設定データをロード
    /// </summary>
    public void LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, FileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            currentSettings = JsonUtility.FromJson<ModalSettings>(json);
        }
        else
        {
            // デフォルト値で設定を初期化
            currentSettings = new ModalSettings
            {
                MinValue = defaultMin,
                MaxValue = defaultMax,
                ShouldConsume = shouldConsume,
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
