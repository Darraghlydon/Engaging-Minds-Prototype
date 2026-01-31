using UnityEngine;
using Ink.Runtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class DialogueManager : MonoBehaviour
{
    public TextAsset inkFile;
    public TextMeshProUGUI textBox;
    private Story story;
    private bool isDialoguePlaying;

    public Button[] choiceButtons;
   // [SerializeField] private GameObject canvasToActivate; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        story = new Story(inkFile.text);
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ContinueStory(); // text progresses at players own pace
        }
    }

    public void StartDialogue() 
    { 
        
        
            if (isDialoguePlaying) return;

            isDialoguePlaying = true;
            story = new Story(inkFile.text);

            
          //  textBox.gameObject.SetActive(true);

            ContinueStory();
        

    }
    private void ContinueStory()
    {
        if (story.canContinue)
        {
            textBox.gameObject.SetActive(true);
            textBox.text = story.Continue();
            ShowChoices();
        }
        else
        {
            FinishDialogue();
        }
    }

    private void ShowChoices()
    {
        List<Choice> choices = story.currentChoices;
        int index = 0;
        foreach (Choice c in choices)
        {
            choiceButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = c.text;
            choiceButtons[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }
    }

    public void SetDecision(int choiceIndex)
    {
        story.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
    private void FinishDialogue()
    {
       textBox.gameObject.SetActive(false);
      
      //foreach (var button in choiceButtons)
         // button.gameObject.SetActive(false);
      isDialoguePlaying = false;
    }
}