#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

[CustomEditor(typeof(TestScriptableObject))]
public class TestScriptableObjectEditor : Editor
{
    private SerializedProperty filePathProperty; // ファイルパス用プロパティ
    private SerializedObject serializedObjectInstance;
    private int _counter = 0;

    public override VisualElement CreateInspectorGUI()
    {
        // SerializedObject を初期化
        serializedObjectInstance = new SerializedObject(target);
        filePathProperty = serializedObjectInstance.FindProperty("filePath");

        // 親コンテナ
        var container = new VisualElement();

        // パス入力フィールド
        var pathField = new PropertyField(filePathProperty, "UXML ファイルパス");
        pathField.Bind(serializedObjectInstance); // バインド
        container.Add(pathField);

        // UXML の読み込みボタン
        var loadButton = new Button(() =>
        {
            string filePath = filePathProperty.stringValue;

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("ファイルパスが指定されていません！");
                return;
            }

            if (!AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(filePath))
            {
                Debug.LogError($"指定されたパスのファイルが見つかりません: {filePath}");
                return;
            }

            // ファイルをロードしてインスタンスに追加
            var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(filePath);
            if (treeAsset != null)
            {
                var uxmlContent = treeAsset.Instantiate();
                container.Add(uxmlContent);
                Debug.Log($"UXML をロードしました: {filePath}");
            }
        })
        {
            text = "UXML をロード"
        };
        container.Add(loadButton);

        // プロパティを全て表示
        var fieldContainer = new VisualElement();
        SerializedProperty property = serializedObjectInstance.GetIterator();
        if (property != null)
        {
            property.NextVisible(true); // 最初のプロパティに移動（`m_Script`をスキップ）
            while (property.NextVisible(false))
            {
                if (property.name != "filePath") // filePath フィールドは重複表示しない
                {
                    var propertyField = new PropertyField(property);
                    propertyField.Bind(serializedObjectInstance);
                    fieldContainer.Add(propertyField);
                }
            }
        }
        container.Add(fieldContainer);

        // UQuery を使用して Label と Button のインスタンスを取得
        var label = container.Q<Label>();
        var button = container.Q<Button>();

        // Button がクリックされた際の動作を追加
        button.clicked += () =>
        {
            // 数値をインクリメントし、Label に反映させます
            _counter++;
            if(label != null )
                label.text = _counter.ToString();
        };

        return container;
    }
}

#endif