using System;
using System.Collections.Generic;
using UnityEngine;

public class ADVCsvReader
{
    private TextAsset layersCsv = null;
    private TextAsset characterCsv = null;
    private TextAsset textureCsv = null;
    private TextAsset scenarioLabelCsv = null;
    private TextAsset scenarioCsv = null;

    private List<LayerData> layerDataList;
    private List<CharacterData> characterDataList;
    private List<TextureData> textureDataList;
    private List<ScenarioLabelData> scenarioLabelDataList;
    private List<ScenarioData> scenarioDataList;

    public ADVCsvReader(TextAsset layersCsv, TextAsset characterCsv, TextAsset textureCsv, TextAsset scenarioLabelCsv, TextAsset scenarioCsv)
    {
        this.layersCsv = layersCsv;
        this.characterCsv = characterCsv;
        this.textureCsv = textureCsv;
        this.scenarioLabelCsv = scenarioLabelCsv;
        this.scenarioCsv = scenarioCsv;
        ReadCSV();
    }

    public void ReadCSV()
    {
        // CSVファイルを読み込む
        layerDataList = ReadLayerCsv(layersCsv);
        characterDataList = ReadCharacterCsv(characterCsv);
        textureDataList = ReadTextureCsv(textureCsv);
        scenarioLabelDataList = ReadScenarioLabelCsv(scenarioLabelCsv);
        scenarioDataList = ReadScenarioCsv(scenarioCsv);
    }

    public UIReflection PrintScenarioDetails(int scenarioId, int eventOrder)
    {
        // ScenarioDataを検索
        ScenarioData scenario = null;
        int nextIndex = 0;
        for (int i = 0; i < scenarioDataList.Count; ++i)
        {
            scenario = scenarioDataList[i];
            if (scenario.ScenarioId == scenarioId && scenario.EventOrder == eventOrder)
            {
                nextIndex = i + 1;
                nextIndex = nextIndex >= scenarioDataList.Count ? -1 : nextIndex;
                break;
            }
        }

        if (scenario == null)
            return null;

        CharacterData character = FindData(characterDataList, c => c.SpriteName == scenario.Argument1);
        LayerData layer = FindData(layerDataList, l => l.Id.ToString() == scenario.Argument2);
        TextureData texture = FindData(textureDataList, t => t.Id.ToString() == scenario.Argument1);
        ScenarioLabelData labelData = FindData(scenarioLabelDataList, l => l.ScenarioId.ToString() == scenario.Argument2);
        ScenarioData nextScenario = nextIndex >= 0 ? scenarioDataList[nextIndex] : null;

        Debug.Log($"ScenarioId: {scenario.ScenarioId}, EventOrder: {scenario.EventOrder}, Text: {scenario.Text}, Command: {scenario.Command}, Arg1:{scenario.Argument1}, Arg2:{scenario.Argument2}");

        if (character != null)
        {
            Debug.Log($"Character Name: {character.Name}, SpriteName: {character.SpriteName}, Width: {character.Width}, Height: {character.Height}");
        }

        if (layer != null)
        {
            Debug.Log($"Layer Name: {layer.Name}, Position: ({layer.X}, {layer.Y}), Order: {layer.Order}");
        }

        if (texture != null)
        {
            Debug.Log($"Texture Name: {texture.ImageName}, Type: {texture.TextureType}, Size: {texture.Size}");
        }

        if (labelData != null)
        {
            Debug.Log($"Scenario Label Name: {labelData.ScenarioName}, SceneId: {labelData.SceneId}");
        }

        return new UIReflection(layer, character, texture, labelData, scenario, nextScenario);
    }

    private T FindData<T>(List<T> dataList, Predicate<T> match) where T : class
    {
        return dataList.Find(match);
    }

    [System.Serializable]
    public class LayerData
    {
        public int Id;
        public string Name;
        public float X;
        public float Y;
        public int Order;
    }

    [System.Serializable]
    public class CharacterData
    {
        public int Id;
        public string Name;
        public string SpriteName;
        public float Width;
        public float Height;
        public int CharacterId;
    }

    [System.Serializable]
    public class TextureData
    {
        public int Id;
        public string ImageName;
        public string TextureType;
        public float Size;
    }

    [System.Serializable]
    public class ScenarioLabelData
    {
        public int ScenarioId;
        public int SceneId;
        public string ScenarioName;
    }

    [System.Serializable]
    public class ScenarioData
    {
        public int ScenarioId;       // シナリオ番号
        public int EventOrder;       // シナリオ番号におけるindex（1-1,3-2など）
        public string Command;       // 処理文字列
        public string Text;          // ウィンドウに反映する文字列
        public string Voice;         // 今は使用しない
        public string Argument1;        // CharacterData(ID)
        public string Argument2;        // LayerDataを参照
        public string Argument3;        // 番号（列挙体で代用）
        public string Action;        // 今は使用しない
        public string BranchName;    // 今は使用しない
    }

    public class UIReflection
    {
        private LayerData layerData = null;
        public LayerData LayerData { get => layerData; }

        private CharacterData characterData = null;
        public CharacterData CharacterData { get => characterData; }
        
        private TextureData textureData = null;
        public TextureData TextureData { get => textureData; }
        
        private ScenarioLabelData scenarioLabelData = null;
        public ScenarioLabelData ScenarioLabelData { get => scenarioLabelData; }
        
        private ScenarioData scenarioData = null;
        public ScenarioData ScenarioData { get => scenarioData; }

        private int nextNo = -1;
        public int NextNo { get => nextNo; set => nextNo = value; }
        private int nextOrder = -1;
        public int NextOrder { get => nextOrder; set => nextOrder = value; }

        public UIReflection(LayerData layerData, CharacterData characterData, TextureData textureData, ScenarioLabelData scenarioLabelData, ScenarioData scenarioData, ScenarioData next)
        {
            this.layerData = layerData;
            this.characterData = characterData;
            this.textureData = textureData;
            this.scenarioLabelData = scenarioLabelData;
            this.scenarioData = scenarioData;
            if(next != null)
            {
                this.nextNo = next.ScenarioId;
                this.nextOrder = next.EventOrder;
            }
        }
    }

    private List<T> ReadCsv<T>(TextAsset csvFile, string[] headers, System.Func<string[], T> createData) where T : new()
    {
        var dataList = new List<T>();
        string[] lines = csvFile.text.Split('\n');
        var headerIndexes = GetHeaderIndexes(lines[0], headers);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] fields = lines[i].Split(',');

            if (CheckSkip(fields[0])) continue;

            try
            {
                // データ作成処理
                var data = createData(fields);
                dataList.Add(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing line {i}: {ex.Message}");
            }
        }

        return dataList;
    }


    private Dictionary<string, int> GetHeaderIndexes(string headerLine, string[] headers)
    {
        var headerIndexes = new Dictionary<string, int>();
        string[] headerFields = headerLine.Split(',');

        for (int i = 0; i < headerFields.Length; i++)
        {
            string header = headerFields[i].Trim();

            // ヘッダーが一致しているか確認
            if (System.Array.Exists(headers, h => h == header))
            {
                headerIndexes[header] = i;
            }
            else
            {
                Debug.LogWarning($"Header '{header}' not found in expected headers.");
            }
        }

        return headerIndexes;
    }


    private T CreateDataFromFields<T>(string[] fields, Dictionary<string, int> headerIndexes) where T : new()
    {
        var data = new T();

        foreach (var headerIndex in headerIndexes)
        {
            string header = headerIndex.Key;
            int index = headerIndex.Value;

            // フィールド名に基づいてデータを適切にセットする
            var fieldValue = fields[index].Trim();

            // Reflectionでフィールドにアクセス
            var field = typeof(T).GetField(header);
            if (field != null)
            {
                try
                {
                    // フィールドが見つかれば、その型に合わせて値を設定
                    field.SetValue(data, Convert.ChangeType(fieldValue, field.FieldType));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error setting field {header} with value {fieldValue}: {ex.Message}");
                }
            }
            else
            {
                // フィールドが存在しない場合、警告を出力
                Debug.LogWarning($"Field {header} not found in {typeof(T).Name}");
            }
        }

        return data;
    }


    private List<LayerData> ReadLayerCsv(TextAsset csvFile)
    {
        string[] headers = { LayerHeaders.Id, LayerHeaders.Name, LayerHeaders.X, LayerHeaders.Y, LayerHeaders.Order };
        return ReadCsv(csvFile, headers, (fields) => {
            var data = new LayerData
            {
                Id = int.Parse(fields[0]),
                Name = fields[1],
                X = float.Parse(fields[2]),
                Y = float.Parse(fields[3]),
                Order = int.Parse(fields[4])
            };
            return data;
        });
    }

    private List<CharacterData> ReadCharacterCsv(TextAsset csvFile)
    {
        string[] headers = { CharacterHeaders.Id, CharacterHeaders.Name, CharacterHeaders.SpriteName, CharacterHeaders.Width, CharacterHeaders.Height, CharacterHeaders.CharacterId };
        return ReadCsv(csvFile, headers, (fields) => {
            var data = new CharacterData
            {
                Id = int.Parse(fields[0]),
                Name = fields[1],
                SpriteName = fields[2],
                Width = float.Parse(fields[3]),
                Height = float.Parse(fields[4]),
                CharacterId = int.Parse(fields[5])
            };
            return data;
        });
    }

    private List<TextureData> ReadTextureCsv(TextAsset csvFile)
    {
        string[] headers = { TextureHeaders.Id, TextureHeaders.ImageName, TextureHeaders.TextureType, TextureHeaders.Size };
        return ReadCsv(csvFile, headers, (fields) => {
            var data = new TextureData
            {
                Id = int.Parse(fields[0]),
                ImageName = fields[1],
                TextureType = fields[2],
                Size = float.Parse(fields[3])
            };
            return data;
        });
    }

    private List<ScenarioLabelData> ReadScenarioLabelCsv(TextAsset csvFile)
    {
        string[] headers = { ScenarioLabelHeaders.ScenarioId, ScenarioLabelHeaders.SceneId, ScenarioLabelHeaders.ScenarioName };
        return ReadCsv(csvFile, headers, (fields) => {
            var data = new ScenarioLabelData
            {
                ScenarioId = int.Parse(fields[0]),
                SceneId = int.Parse(fields[1]),
                ScenarioName = fields[2]
            };
            return data;
        });
    }

    private List<ScenarioData> ReadScenarioCsv(TextAsset csvFile)
    {
        string[] headers =
        {
            ScenarioHeaders.ScenarioId,
            ScenarioHeaders.EventOrder,
            ScenarioHeaders.Command,
            ScenarioHeaders.Argument1,
            ScenarioHeaders.Argument2,
            ScenarioHeaders.Argument3,
            ScenarioHeaders.Text,
            ScenarioHeaders.Action,
            ScenarioHeaders.Voice,
            ScenarioHeaders.BranchName,
        };

        return ReadCsv(csvFile, headers, (fields) => {
            var data = new ScenarioData();

            // Try parsing integers safely
            bool success;
            data.ScenarioId = TryParseInt(fields[Array.IndexOf(headers, ScenarioHeaders.ScenarioId)], out success);
            if (!success)
                Debug.LogError($"Invalid ScenarioId format at line {Array.IndexOf(fields, fields)}");

            data.Text = fields[Array.IndexOf(headers, ScenarioHeaders.Text)];
            data.EventOrder = TryParseInt(fields[Array.IndexOf(headers, ScenarioHeaders.EventOrder)], out success);
            if (!success)
                Debug.LogError($"Invalid EventOrder format at line {Array.IndexOf(fields, fields)}");

            data.Voice = fields[Array.IndexOf(headers, ScenarioHeaders.Voice)];
            data.Argument1 = fields[Array.IndexOf(headers, ScenarioHeaders.Argument1)];
            data.Argument2 = fields[Array.IndexOf(headers, ScenarioHeaders.Argument2)];
            data.Argument3 = fields[Array.IndexOf(headers, ScenarioHeaders.Argument3)];
            data.Action = fields[Array.IndexOf(headers, ScenarioHeaders.Action)];
            data.BranchName = fields[Array.IndexOf(headers, ScenarioHeaders.BranchName)];
            data.Command = fields.Length > Array.IndexOf(headers, ScenarioHeaders.Command) ? fields[Array.IndexOf(headers, ScenarioHeaders.Command)] : "";

            return data;
        });
    }

    private int TryParseInt(string value, out bool success)
    {
        int result;
        success = int.TryParse(value, out result);
        return result;
    }


    private bool CheckSkip(string text)
    {
        return text == "#" || text == "//";
    }
}
