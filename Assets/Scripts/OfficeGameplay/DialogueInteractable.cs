using UnityEngine;

namespace OfficeGameplay
{
    // makes a game object interactable
    public class DialogueInteractable : MonoBehaviour, IInteractable
    {
        //called by InteractionController when the player presses E while looking at this object. Starts the dialogue sequence via the DialogueManager.
        public string InteractMessage => interactMessage;

        [SerializeField] string interactMessage; 
        [SerializeField] private DialogueManager dialogueManager;
      

        public void Interact()
        {
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
            }
        }
    }
}
