#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SetPivotToParentImageCenterWindow : EditorWindow
{
    private RectTransform selectedRectTransform;

    [MenuItem("Tools/Set Pivot to Parent Image's Center")]
    public static void ShowWindow()
    {
        // ウィンドウを表示
        SetPivotToParentImageCenterWindow window = GetWindow<SetPivotToParentImageCenterWindow>();
        window.titleContent = new GUIContent("Set Pivot");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Pivot to Parent Image's Center", EditorStyles.boldLabel);

        // 選択されたRectTransformを表示
        selectedRectTransform = Selection.activeGameObject?.GetComponent<RectTransform>();

        if (selectedRectTransform != null)
        {
            // 親のImageを取得
            var parentImage = selectedRectTransform.parent?.GetComponent<Image>();

            if (parentImage != null)
            {
                // ピボット設定ボタン
                if (GUILayout.Button("Set Pivot to Parent Image's Center"))
                {
                    Undo.RecordObject(selectedRectTransform, "Set Pivot to Parent Image's Center");
                    var pivot = new Vector2(
                        parentImage.sprite.pivot.x / parentImage.sprite.rect.width,
                        parentImage.sprite.pivot.y / parentImage.sprite.rect.height
                    );
                    Vector2 parentPos = parentImage.rectTransform.position;
                    selectedRectTransform.anchoredPosition = Vector2.zero - pivot;
                    EditorUtility.SetDirty(selectedRectTransform);
                }
            }
            else
            {
                GUILayout.Label("Parent does not have an Image component.");
            }
        }
        else
        {
            GUILayout.Label("Please select a RectTransform.");
        }
    }
}

#endif