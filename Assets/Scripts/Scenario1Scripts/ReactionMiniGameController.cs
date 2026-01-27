using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionMinigameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;     // ReactionMinigamePanel
    [SerializeField] private RectTransform barRect;    // Bar rect
    [SerializeField] private RectTransform zoneRect;   // SuccessZone rect
    [SerializeField] private RectTransform arrowRect;  // Arrow rect

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    // Optional: disable your normal gameplay action map while minigame runs
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string gameplayActionMapName = "Player"; // change to yours

    private Action<bool> onComplete;
    private ReactionMinigameProfile profile;

    private float t;          // arrow normalized position (0..1)
    private float dir = 1f;   // +1 up, -1 down
    private float elapsed;
    private bool running;

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;
    }

    public void Show(ReactionMinigameProfile profile, Action<bool> onComplete)
    {
        SetActionMap("Player", false);
        SetActionMap("UI", true);

        this.profile = profile;
        this.onComplete = onComplete;

        t = UnityEngine.Random.value; // random start feels nicer than always bottom
        dir = 1f;
        elapsed = 0f;
        running = true;

        ApplyZoneUI(profile);

        if (panelRoot != null) panelRoot.SetActive(true);

        // Disable gameplay map so Interact doesn't also trigger other gameplay things
        SetGameplayEnabled(false);

        // Make sure interact action is enabled
        interactAction?.action.Enable();
    }

    private void End(bool success)
    {
        if (!running) return;
        running = false;

        if (panelRoot != null) panelRoot.SetActive(false);
        SetGameplayEnabled(true);

        var cb = onComplete;
        onComplete = null;
        profile = null;

        cb?.Invoke(success);
    }

    void Update()
    {
        if (!running || profile == null) return;

        // Move arrow up/down (ping-pong bounce)
        float delta = profile.speed * Time.unscaledDeltaTime; // unscaled for UI minigame
        t += dir * delta;

        if (t >= 1f) { t = 1f; dir = -1f; }
        if (t <= 0f) { t = 0f; dir = 1f; }

        ApplyArrowUI(t);

        // Optional timeout
        if (profile.timeLimit > 0f)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= profile.timeLimit)
                End(false);
        }
    }

    private void SetActionMap(string mapName, bool enabled)
    {
        var map = inputActions.FindActionMap(mapName, true);
        if (map == null) return;

        if (enabled) map.Enable();
        else map.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!running || profile == null) return;

        // success if arrow is within zone
        float half = profile.zoneSize * 0.5f;
        float min = Mathf.Clamp01(profile.zoneCenter - half);
        float max = Mathf.Clamp01(profile.zoneCenter + half);

        bool success = (t >= min && t <= max);
        Debug.Log("Game Success: " + success);
        End(success);
    }

    private void ApplyZoneUI(ReactionMinigameProfile p)
    {
        float half = p.zoneSize * 0.5f;
        float min = Mathf.Clamp01(p.zoneCenter - half);
        float max = Mathf.Clamp01(p.zoneCenter + half);

        // Fill width; occupy min..max vertically
        zoneRect.anchorMin = new Vector2(0f, min);
        zoneRect.anchorMax = new Vector2(1f, max);

        zoneRect.offsetMin = Vector2.zero;
        zoneRect.offsetMax = Vector2.zero;
    }

    private void ApplyArrowUI(float normalized)
    {
        // Pointer at center X, at Y = normalized
        arrowRect.anchorMin = new Vector2(0.5f, normalized);
        arrowRect.anchorMax = new Vector2(0.5f, normalized);
        arrowRect.anchoredPosition = Vector2.zero;
    }

    private void SetGameplayEnabled(bool enabled)
    {
        SetActionMap("UI", false);
        SetActionMap("Player", true);
        if (inputActions == null || string.IsNullOrEmpty(gameplayActionMapName)) return;

        var map = inputActions.FindActionMap(gameplayActionMapName, true);
        if (map == null) return;

        if (enabled) map.Enable();
        else map.Disable();
    }
}
