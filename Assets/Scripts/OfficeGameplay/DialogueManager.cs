using System.Collections;
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
    public GameObject dialogueCanvas;
    private Story story;
    private bool dialogueOpen;
  
   void Awake()
   {
      
       if (inkFile != null)
       {
           story = new Story(inkFile.text);

       }

       dialogueOpen = true;
       dialogueCanvas.SetActive(false);
       
   }
   
   
   
  public void StartDialogue()
  {
      StartCoroutine(StartDialogueRoutine());
  }

  IEnumerator StartDialogueRoutine()
  {
      dialogueCanvas.SetActive(true);
      textBox.gameObject.SetActive(true);

      yield return null; // wait one frame

      if (story != null && story.canContinue)
      
          textBox.text = story.Continue();
          textBox.ForceMeshUpdate();
      
  }

  // continue to read dialogue
  void Update()
  {
      if (!dialogueCanvas.activeSelf)
          return;
      

      if (Keyboard.current.spaceKey.wasPressedThisFrame)
      {
          if (story != null && story.canContinue)
          {
              textBox.text = story.Continue();
              textBox.ForceMeshUpdate();
          }
          else
          {
              FinishDialogue();
          }
      }
  }

   
// hide dialogue canvas when text finishes
      void FinishDialogue() 
        {
            dialogueCanvas.SetActive(false);
            textBox.gameObject.SetActive(false);
          
        }
     
    
    }
