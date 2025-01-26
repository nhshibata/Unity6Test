using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CsvReader;

public class ADVView : MonoBehaviour
{
    private enum AdvMassage
    {
        Save, 
        Load,
        Skip,
    }

    [SerializeField]
    private Image backGround = null;

    [Header("Window")]
    [SerializeField]
    private TextMeshProUGUI characterNameText = null;
    [SerializeField]
    private TextMeshProUGUI dialogueText = null;
    [SerializeField]
    private Button massagePanel = null;
    [SerializeField]
    private Button waitPanel = null;
    [SerializeField]
    private RectTransform tapWaitUI = null;
    [SerializeField]
    private CanvasGroup menuContents = null;
    
    [Header("Popup")]
    [SerializeField]
    private MessageLog messageLog = null;
    [SerializeField]
    private PopupMessage popup = null; 
    [SerializeField]
    private YesNoDialog dialog = null;
    [SerializeField]
    private ConfigView configView = null;
    [SerializeField]
    private FadeManager fadeManager = null;
    public FadeManager FadeManager { get => fadeManager; }

    [Header("MenuButtons")]
    [SerializeField]
    private Button skipButton = null;
    [SerializeField]
    private Toggle autoToggle = null;
    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private Button loadButton = null;
    [SerializeField]
    private Button quickSaveButton = null;
    [SerializeField]
    private Button quickLoadButton = null;
    [SerializeField]
    private Button configButton = null;
    [SerializeField]
    private Button logButton = null;
    [SerializeField]
    private Toggle menuSwitchToggle = null;

    [SerializeField]
    private List<Image> characters = new List<Image>();
    [SerializeField]
    private SerializableDictionary<AdvMassage, string> massageMap = new SerializableDictionary<AdvMassage, string>();

    private ADVInput inputs = null;
    private TextNextIndicator textNextIndicator = null;
    private Dictionary<int, int> posIndex = new Dictionary<int, int>();

    public Action OnSkipPressed = null;
    public Action<bool> OnAutoToggleChanged = null;
    public Action OnSavePressed = null;
    public Action OnLoadPressed = null;
    public Action OnQuickSavePressed = null;
    public Action OnQuickLoadPressed = null;
    public Action OnConfigPressed = null;
    public Action OnLogButtonPressed = null;
    public Action OnNextButtonPressed = null;


    private void Awake()
    {
        // ボタンにイベントを設定
        autoToggle.onValueChanged.AddListener(isOn => OnAutoToggleChanged?.Invoke(isOn));
        saveButton.onClick.AddListener(() => OnSavePressed?.Invoke());
        loadButton.onClick.AddListener(() => OnLoadPressed?.Invoke());
        quickSaveButton.onClick.AddListener(() => OnQuickSavePressed?.Invoke());
        quickLoadButton.onClick.AddListener(() => OnQuickLoadPressed?.Invoke());
        configButton.onClick.AddListener(() => OnConfigPressed?.Invoke());
        logButton.onClick.AddListener(() => OnLogButtonPressed?.Invoke());
        massagePanel.onClick.AddListener(() => OnNextButtonPressed?.Invoke());

        // Skip押下でダイアログを表示
        skipButton.onClick.AddListener(() => {
            dialog.ShowYesNo(massageMap[AdvMassage.Skip], (value) => {
                if(value == 0)
                {
                    OnSkipPressed?.Invoke();
                }

                _ = dialog.HideDialog();
            });
        });

        // コンフィグ表示
        OnConfigPressed += () => {
            configView.Show();
        };

        // ログ表示
        OnLogButtonPressed += () => {
            messageLog.Show();
        };

        // Autoボタン
        OnAutoToggleChanged += (value) => {
            waitPanel.image.raycastTarget = value;
            autoToggle.targetGraphic.color = value ? Color.red : Color.white;
        };

        waitPanel.onClick.AddListener(() => {
            OnStop();
            waitPanel.image.raycastTarget = false;
        });

        // セーブ成功表示
        OnQuickSavePressed += () => {
            _ = popup.ShowMessage(massageMap[AdvMassage.Save]);
        };

        // ロード成功表示
        OnQuickLoadPressed += () => {
            _ = popup.ShowMessage(massageMap[AdvMassage.Load]);
        };

        menuSwitchToggle.onValueChanged.AddListener((value) => {
            menuContents.alpha  = (value ?  1 : 0);
            menuContents.interactable  = value;
        });

        // Escキーでウィンドウ表示を切り替える処理
        // ※必要なら Input System に置き換え
        inputs = new ADVInput();
        inputs.Enable();
        inputs.Window.SwitchEnable.performed += (a) => {
            if (menuContents.gameObject.activeSelf)
            {
                Hide();
                messageLog.Hide(); // ログウィンドウを非表示
            }
            else
            {
                menuContents.gameObject.SetActive(true);
            }
        };

        textNextIndicator = new TextNextIndicator();
        textNextIndicator.StartTapIconAnimation(tapWaitUI);
    }

    private void OnDisable()
    {
        messageLog.ClearLog();
        textNextIndicator.StopTapIconAnimation(tapWaitUI);
    }

    public void OnStop()
    {
        autoToggle.isOn = false;
    }

    public void Hide()
    {
        menuContents.gameObject.SetActive(false);
    }

    public void Show(string characterName, string dialogue)
    {
        characterNameText.text = characterName;
        dialogueText.text = dialogue;
        menuContents.gameObject.SetActive(true);
    }

    public void ShowDialogueWithEffect(string charaName, string text, float delayPerChar)
    {
        StopAllCoroutines(); // 既存のコルーチンを停止
        StartCoroutine(TypeDialogueEffect(charaName, text, delayPerChar));
    }

    private System.Collections.IEnumerator TypeDialogueEffect(string charaName, string text, float delayPerChar)
    {
        characterNameText.text = charaName;
        dialogueText.text = text;
        messageLog.AddToLog(charaName, text);
        tapWaitUI.gameObject.SetActive(false);

        for (int i = 0; i < text.Length; i++)
        {
            // 徐々に表示文字数を増やしていく
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delayPerChar);
        }

        dialogueText.maxVisibleCharacters = text.Length;
        tapWaitUI.gameObject.SetActive(true); // 完了後にタップ待機UIを表示
    }

    public void DataToUpdate(UIReflection reflection)
    {
        // TODO: 挙動は他ファイルに分割する
        if(reflection.ScenarioData.Command == "Character")
        {
            int posID = reflection.LayerData.Id;
            if (!posIndex.ContainsKey(posID))
            {
                int charaID = 0;
                for (int i = 0; i < characters.Count; i++)
                {
                    if (!characters[i].gameObject.activeSelf)
                    {
                        charaID = i;
                        break;
                    }
                }

                posIndex.Add(posID, charaID);
            }

            // キャラクター表示切り替え
            int characterIndex = posIndex[posIndex[posID]];
            
            characters[characterIndex].rectTransform.position = new Vector3(reflection.LayerData.X, reflection.LayerData.Y, 0);
            characters[characterIndex].rectTransform.sizeDelta = new Vector2(reflection.CharacterData.Width, reflection.CharacterData.Height);
            //characters[characterIndex].sprite = reflection.CharacterData.SpriteName // 一致するSpriteを取得して反映;
            //reflection.LayerData.Order 描画順を変更(1~)
        }
        else if(reflection.ScenarioData.Command == "Bg")
        {
            //backGround.sprite = reflection.TextureData.ImageName; 一致するSpriteを取得
            backGround.transform.localScale = Vector3.one * reflection.TextureData.Size;
        }
        else if(reflection.ScenarioData.Command == "StartScenario")
        {
            // fade処理
            // 次のorderを取得し反映する
        }
        else if(reflection.ScenarioData.Command == "EndScenario")
        {
            // fade処理
            // UIを初期化する
        }
        else if(reflection.ScenarioData.Command == string.Empty)
        {
            dialogueText.text = reflection.ScenarioData.Text;
        }

    }

}
