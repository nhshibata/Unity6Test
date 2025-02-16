using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> sprites = new List<Sprite>();

    private Dictionary<string, Sprite> spriteDictionary;

    private void Awake()
    {
        Initialize(sprites);
    }

    public void Initialize(List<Sprite> sprites)
    {
        spriteDictionary = new Dictionary<string, Sprite>();

        // SpriteをDictionaryに追加
        foreach (var sprite in sprites)
        {
            // Spriteの名前（または任意の識別子）をキーとして保存
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sprite.name);
            spriteDictionary[fileNameWithoutExtension] = sprite;
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        if (spriteDictionary.ContainsKey(name))
        {
            return spriteDictionary[name];
        }

        Debug.LogWarning("Sprite not found for name: " + name);
        return null;
    }
}
