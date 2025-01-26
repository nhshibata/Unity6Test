using System;
using System.Collections.Generic;
using UnityEngine;

public class CsvReader
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

    public CsvReader(TextAsset layersCsv, TextAsset characterCsv, TextAsset textureCsv, TextAsset scenarioLabelCsv, TextAsset scenarioCsv)
    {
        this.layersCsv = layersCsv;
        this.characterCsv = characterCsv;
        this.textureCsv = textureCsv;
        this.scenarioLabelCsv = scenarioLabelCsv;
        this.scenarioCsv = scenarioCsv;
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

        CharacterData character = FindData(characterDataList, c => c.Id == scenario.Argument1);
        LayerData layer = FindData(layerDataList, l => l.Id == scenario.Argument2);
        TextureData texture = FindData(textureDataList, t => t.Id == scenario.Argument1);
        ScenarioLabelData labelData = FindData(scenarioLabelDataList, l => l.ScenarioId == scenario.Argument2);
        ScenarioData nextScenario = nextIndex >= 0 ? scenarioDataList[nextIndex] : null;

        Debug.Log($"ScenarioId: {scenario.ScenarioId}, EventOrder: {scenario.EventOrder}, Text: {scenario.Text}");

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
        public int Argument1;        // CharacterData(ID)
        public int Argument2;        // LayerDataを参照
        public int Argument3;        // 番号（列挙体で代用）
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

            var data = createData(fields);
            dataList.Add(data);
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
            if (System.Array.Exists(headers, h => h == header))
            {
                headerIndexes[header] = i;
            }
        }
        return headerIndexes;
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
            ScenarioHeaders.Text,
            ScenarioHeaders.EventOrder,
            ScenarioHeaders.Voice,
            ScenarioHeaders.Argument1,
            ScenarioHeaders.Argument2,
            ScenarioHeaders.Argument3,
            ScenarioHeaders.Action,
            ScenarioHeaders.BranchName,
            ScenarioHeaders.Command
        };

        return ReadCsv(csvFile, headers, (fields) => {
            var data = new ScenarioData
            {
                ScenarioId = int.Parse(fields[Array.IndexOf(headers, ScenarioHeaders.ScenarioId)]),
                Text = fields[Array.IndexOf(headers, ScenarioHeaders.Text)],
                EventOrder = int.Parse(fields[Array.IndexOf(headers, ScenarioHeaders.EventOrder)]),
                Voice = fields[Array.IndexOf(headers, ScenarioHeaders.Voice)],
                Argument1 = int.Parse(fields[Array.IndexOf(headers, ScenarioHeaders.Argument1)]),
                Argument2 = int.Parse(fields[Array.IndexOf(headers, ScenarioHeaders.Argument2)]),
                Argument3 = int.Parse(fields[Array.IndexOf(headers, ScenarioHeaders.Argument3)]),
                Action = fields[Array.IndexOf(headers, ScenarioHeaders.Action)],
                BranchName = fields[Array.IndexOf(headers, ScenarioHeaders.BranchName)],
                Command = fields.Length > Array.IndexOf(headers, ScenarioHeaders.Command) ? fields[Array.IndexOf(headers, ScenarioHeaders.Command)] : ""
            };
            return data;
        });
    }

    private bool CheckSkip(string text)
    {
        return text == "#" || text == "//";
    }
}
