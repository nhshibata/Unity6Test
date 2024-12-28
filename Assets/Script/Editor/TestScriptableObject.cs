#if UNITY_EDITOR

using UnityEngine;

[CreateAssetMenu(fileName = "UXML", menuName = "ScriptableObjects/UXML")]
public class TestScriptableObject : ScriptableObject
{
    public string filePath = "Assets/UXML/test1.uxml"; // UXMLファイルのパスを指定するフィールド
    public int someValue;   // 他のプロパティ
}

#endif