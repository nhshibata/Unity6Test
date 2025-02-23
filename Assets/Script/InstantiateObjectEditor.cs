using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InstantiateObjects))]
public class InstantiateObjectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InstantiateObjects script = (InstantiateObjects)target;

        if (GUILayout.Button("Generate Objects"))
        {
            script.GenerateObjects();
        }

        if (GUILayout.Button("Delete Generated Objects"))
        {
            script.DeleteGeneratedObjects();
        }
    }
}
