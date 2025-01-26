using System.Collections.Generic;
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
            spriteDictionary[sprite.name] = sprite;
        }
    }

    public Sprite GetSpriteByName(string name)
    {
        // 名前に一致するSpriteを返す
        if (spriteDictionary.TryGetValue(name, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning("Sprite not found for name: " + name);
            return null;
        }
    }
}
