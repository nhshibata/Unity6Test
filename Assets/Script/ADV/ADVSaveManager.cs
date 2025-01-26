using UnityEngine;


[System.Serializable]
public class SaveData
{
    public int currentScenarioID;
}

public class ADVSaveManager : MonoBehaviour
{
    private const string SaveKey = "SaveData";

    public void Save(SaveData data)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
    }

    public SaveData Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            return JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));
        }
        return null;
    }
}
