using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteManager : CharacterSpriteManagerBase
{
    [Header("Refarence")]
    [SerializeField]
    private List<Sprite> charSprites;

    [Header("Parameters")]
    [SerializeField]
    private int defaultFace = 0;


    public void Awake()
    {
        ChangeSprite(charSprites[defaultFace]);
    }

    public override void ChangeToRandomCharacter()
    {
        int index = UnityEngine.Random.Range(defaultFace + 1, charSprites.Count);
        ChangeSprite(charSprites[index]);
    }

    public override void ResetToDefaultCharacter()
    {
        ChangeSprite(charSprites[defaultFace]);
    }
}
