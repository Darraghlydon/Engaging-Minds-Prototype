using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace OfficeGameplay

{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] Camera playerCamera;
        [SerializeField] TextMeshProUGUI interactionText;
        [SerializeField] private float interactionDistance;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private DialogueManager DialogueManagerInstance;

        private IInteractable currentTargetedInteractable;



        // Update is called once per frame
        void Update()
        {
            UpdateCurrentInteractable();
            UpdateCurrentInteractionText();

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame &&
                currentTargetedInteractable != null)
            {
                Debug.Log("INTERACT");
                currentTargetedInteractable.Interact();
            }
        }

        void UpdateCurrentInteractable()
        {
            
            Ray ray = playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                currentTargetedInteractable =
                    hit.collider.GetComponentInParent<IInteractable>();
            }
            else
            {
                currentTargetedInteractable = null;
            }
        }

        void UpdateCurrentInteractionText()
        
        {
           /* interactionText.text =
                currentTargetedInteractable != null
                    ? currentTargetedInteractable.InteractMessage
                    : "";*/
           
           if (DialogueManagerInstance.dialogueCanvas.activeSelf)
           {
               interactionText.gameObject.SetActive(false);
               return;
           }

           
           if (currentTargetedInteractable != null)
           {
               interactionText.gameObject.SetActive(true);
               interactionText.text = currentTargetedInteractable.InteractMessage;
           }
           else
           {
               interactionText.gameObject.SetActive(false);
           }
        }
    }

}