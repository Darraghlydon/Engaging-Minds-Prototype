using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private LayerMask npcLayer = ~0;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "MainGame";
    [SerializeField] private string clickActionName = "Interact";

    private InputAction _clickAction;

    private IInteractable _current;

    public static Action<bool> OnFreeCamToggled;

    private void Reset()
    {
        cam = GetComponent<Camera>();
    }

    private void Awake()
    {
        if (!cam) cam = GetComponent<Camera>();

        if (!inputActions)
        {
            Debug.LogError("LookInteractor: inputActions not assigned.");
            return;
        }

        var map = inputActions.FindActionMap(actionMapName, true);
        _clickAction = map.FindAction(clickActionName, true);
    }

    private void OnEnable()
    {
        if (_clickAction != null)
        {
            _clickAction.performed += OnClickPerformed;
            _clickAction.Enable();
        }

    }

    private void OnDisable()
    {
        if (_clickAction != null)
        {
            _clickAction.performed -= OnClickPerformed;
            _clickAction.Disable();
        }


    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        // For a Button action, performed = press
        TryClickSelectAtPointer();
    }

    private void TryClickSelectAtPointer()
    {
        if (!cam) return;

        // For mouse: current position. For touch: primary touch position.
        // Works if your Click action is triggered by mouse left / touch press.
        Vector2 screenPos = GetPointerScreenPosition();

        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayer, QueryTriggerInteraction.Ignore))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                SetCurrent(interactable);
                _current.Interact(this);
                return;
            }
        }

        // Clicked empty space -> clear selection
        ClearCurrent();
    }

    private Vector2 GetPointerScreenPosition()
    {
        // Prefer EnhancedTouch? Keeping it simple:
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        // Fallback to center
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void SetCurrent(IInteractable next)
    {
        if (next == _current) return;
        _current = next;

        //if (_current != null) _current.OnLookExit(this);
        //if (_current != null) _current.OnLookEnter(this);
    }

    private void ClearCurrent()
    {
        if (_current != null)
        {
            //_current.OnLookExit(this);
            _current = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!cam) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * 2f);
    }
#endif
}
