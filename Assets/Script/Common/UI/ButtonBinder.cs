using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBinder : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text = null;
    [SerializeField]
    private Button button = null;

    public TMP_Text Text { get => text; set => text = value; }
    public Button Button { get => button; set => button = value; }
}
