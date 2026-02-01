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

    public GameObject dialogueCanvas;
    private bool dialogueOpen;
    private bool lineDisplayed;


    void Awake()
    {
        dialogueCanvas.SetActive(false);
        textBox.gameObject.SetActive(false);
        if (inkFile != null)
        {
            story = new Story(inkFile.text);
        }
    }

    void Update()
    {
        if (dialogueCanvas.activeSelf && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (story != null && story.canContinue)
            {
                textBox.text = story.Continue();
            }
            else
            {
                FinishDialogue();
            }
        }
    }

    public void StartDialogue()
         {


             dialogueCanvas.SetActive(true);
             textBox.gameObject.SetActive(true);
             if (story != null && story.canContinue)
                 textBox.text = story.Continue();


         }

       void FinishDialogue()
        {
            dialogueCanvas.SetActive(false);
            textBox.gameObject.SetActive(false);
        }
    }
