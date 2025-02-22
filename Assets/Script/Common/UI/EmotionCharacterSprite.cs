using System.Collections.Generic;
using UnityEngine;

public class EmotionCharacterSprite : CharacterSpriteManagerBase
{
    public enum Emotion
    {
        Default, 
        Positive,
        Negative,
    }

    [Header("Refarence")]
    [SerializeField]
    private SerializableDictionary<Emotion, List<Sprite>> characterSprite = new SerializableDictionary<Emotion, List<Sprite>>();

    [Header("Parameters")]
    [SerializeField]
    private int defaultFace = 0;


    public void ChangeToRandomCharacter(Emotion emotion)
    {
        var list = characterSprite[emotion];
        int index = UnityEngine.Random.Range(0, list.Count);
        ChangeSprite(list[index]);
    }

    public override void ResetToDefaultCharacter()
    {
        var list = characterSprite[Emotion.Default];
        ChangeSprite(list[defaultFace]);
    }
}