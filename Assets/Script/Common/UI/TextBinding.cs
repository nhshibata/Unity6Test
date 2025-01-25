using TMPro;
using UnityEngine;

public class TextBinding : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public TMP_Text Text { get => text; set => text = value; }
}
