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

    private void Start()
    {
        InputManager.Instance.Actions.UI.Interact.performed += OnInteractPerformed;
    }


    void OnDestroy()
    {
        if(InputManager.Instance != null)
        {
            InputManager.Instance.Actions.UI.Interact.performed -= OnInteractPerformed;
        }
        
    }

    public void Show(ReactionMinigameProfile profile, Action<bool> onComplete)
    {
        InputManager.Instance.SwitchToUI();

        this.profile = profile;
        this.onComplete = onComplete;

        t = UnityEngine.Random.value; 
        dir = 1f;
        elapsed = 0f;
        running = true;

        ApplyZoneUI(profile);

        if (panelRoot != null) panelRoot.SetActive(true);

    }

    private void End(bool success)
    {
        if (!running) return;
        running = false;

        InputManager.Instance.SwitchToPlayer();

        if (panelRoot != null) panelRoot.SetActive(false);

        var cb = onComplete;
        onComplete = null;
        profile = null;

        cb?.Invoke(success);
    }

    void Update()
    {
        if (!running || profile == null) return;

        // Move arrow up/down
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

}
