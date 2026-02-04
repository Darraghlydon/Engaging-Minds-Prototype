using System;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;


public class DialogController : MonoBehaviour
{
    public static event Action<Story> OnCreateStory;

    public CanvasGroup canvasgroup;

    private TextAsset currentInkDialogAsset = null;
    public Story story;

    [SerializeField]
    private GameObject dialogPanel = null;

    // UI Prefabs
    [SerializeField]
    private Text textPrefab = null;
    [SerializeField]
    private Button buttonPrefab = null;
    private OfficeNPC currentNPC = null;

    [SerializeField] private Image npcImageUI;
    [SerializeField] private Image playerImageUI;

    private void Awake()
    {
        ToggleDialogCanvas(false);
    }

    private void OnEnable()
    {
        DialogManager.Instance.OnDialogStart += Init;
        GameManager.Instance.DialogueChanged += ToggleDialogCanvas;
    }

    private void OnDisable()
    {
        DialogManager.Instance.OnDialogStart -= Init;
        GameManager.Instance.DialogueChanged -= ToggleDialogCanvas;

    }

    public void Init(OfficeNPC npc)
    {
        currentNPC = npc;
        SetNPCImage(npc.npcImage);
        SetPlayerImage(CharacterManager.Instance.characterData.SelectedPortrait);
        currentInkDialogAsset = npc.TextAsset;

        RemoveChildren();
        StartStory();
    }

    private void SetNPCImage(Sprite npcImage)
    {
        npcImageUI.sprite = npcImage;
    }

    public void SetPlayerImage(Sprite playerImage)
    {
        playerImageUI.sprite = playerImage;
    }

    // Creates a new Story object with the compiled story which we can then play!
    void StartStory()
    {
        story = new Story(currentInkDialogAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);

        string dialogVar = "dialogText";
        object dialogueMessage = currentNPC.DialogueMessage;

        if (story.variablesState.GetVariableWithName(dialogVar) != null)
            story.variablesState[dialogVar] = dialogueMessage;

        ToggleDialogCanvas(true);

        if (story.variablesState.GetVariableWithName("startMinigame") != null)
            story.ObserveVariable("startMinigame", TrackMiniGameStart);

        RefreshView();
    }

    // This is the main function called every time the story changes. It does a few things:
    // Destroys all the old content and choices.
    // Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
    void RefreshView()
    {
        // Remove all the UI on screen
        RemoveChildren();

        // Read all the content until we can't continue any more
        while (story.canContinue)
        {
            // Continue gets the next line of the story
            string text = story.Continue();
            // This removes any white space from the text.
            text = text.Trim();
            // Display the text on screen!
            CreateContentView(text);
        }

        // Display all the choices, if there are any!
        if (story.currentChoices.Count > 0)
        {
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                Button button = CreateChoiceView(choice.text.Trim());
                // Tell the button what to do when we press it
                button.onClick.AddListener(delegate
                {
                    OnClickChoiceButton(choice);
                });
            }
        }
        // If we've read all the content and there's no choices, the story is finished!
        else
        {
            GameManager.Instance?.ExitDialogue();
        }
    }

    public void TrackMiniGameStart(string variableName, object newValue)
    {
        bool playMiniGame = Convert.ToBoolean(newValue);

        if (playMiniGame)
        {
            DialogManager.Instance.RequestMinigame1(currentNPC);
        }
    }

    public void ToggleDialogCanvas(bool toggle)
    {
        canvasgroup.alpha = toggle ? 1 : 0;
        canvasgroup.blocksRaycasts = toggle;
    }

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice)
    {
        story.ChooseChoiceIndex(choice.index);
        RefreshView();
    }

    // Creates a textbox showing the the line of text
    void CreateContentView(string text)
    {
        Text storyText = Instantiate(textPrefab) as Text;
        storyText.text = text;
        storyText.transform.SetParent(dialogPanel.transform, false);
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text)
    {
        // Creates the button from a prefab
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(dialogPanel.transform, false);

        // Gets the text from the button prefab
        Text choiceText = choice.GetComponentInChildren<Text>();
        choiceText.text = text;

        // Make the button expand to fit the text
        HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;

        return choice;
    }


    // Destroys all the children of this gameobject (all the UI)
    void RemoveChildren()
    {
        int childCount = dialogPanel.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            Destroy(dialogPanel.transform.GetChild(i).gameObject);
        }
    }

    internal void OnGameStateChanged(MenuScreen screen)
    {
        canvasgroup.interactable = screen != MenuScreen.Pause;
    }
}
