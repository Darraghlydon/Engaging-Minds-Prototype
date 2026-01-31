using NUnit.Framework;
using UnityEngine;

namespace OfficeGameplay
{
    public class DialogueInteractable : MonoBehaviour, IInteractable
    {
        public string InteractMessage => interactMessage;

        [SerializeField] private string interactMessage = "Press E to talk";
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private GameObject canvasToActivate;

        public void Interact()
        {
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
                Debug.Log("Dialogue started!");
            }
        }
    }
}
