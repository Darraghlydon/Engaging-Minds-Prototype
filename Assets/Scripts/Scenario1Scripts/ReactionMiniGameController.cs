using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionMinigameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _panelRoot;     // ReactionMinigamePanel
    [SerializeField] private RectTransform _barRect;    // Bar rect
    [SerializeField] private RectTransform _zoneRect;   // SuccessZone rect
    [SerializeField] private RectTransform _arrowRect;  // Arrow rect

    private Action<bool> onComplete;
    private ReactionMinigameProfile _profile;

    private float _arrowPosition;          // arrow normalized position (0..1)
    private float _direction = 1f;   // +1 up, -1 down
    private float _elapsed;
    private bool _running;
    private bool _paused;

    void Awake()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void Start()
    {
        InputManager.Instance.Actions.UI.Interact.performed += OnInteractPerformed;
    }

    private void OnEnable()
    {
        Events.Pause.Subscribe(OnPause);
        Events.Pause.Subscribe(OnUnpaused);
    }

    private void OnDisable()
    {
        Events.Pause.Unsubscribe(OnPause);
        Events.Pause.Unsubscribe(OnUnpaused);
    }

    void OnPause()
    {
        _paused = true;
    }

    void OnUnpaused()
    {
        _paused = false;
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
        InputManager.Instance.SwitchToReactionGame();
        Time.timeScale = 0f;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        this._profile = profile;
        this.onComplete = onComplete;

        _arrowPosition = UnityEngine.Random.value;
        _direction = 1f;
        _elapsed = 0f;
        _running = true;

        ApplyZoneUI(profile);

        if (_panelRoot != null) _panelRoot.SetActive(true);

    }

    private void End(bool success)
    {
        if (!_running) return;
        _running = false;
        Time.timeScale = 1f;
        InputManager.Instance.SwitchToMainGame();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        if (_panelRoot != null) _panelRoot.SetActive(false);

        var cb = onComplete;
        onComplete = null;
        _profile = null;

        cb?.Invoke(success);
    }

    void Update()
    {
        if (!_running || _profile == null||_paused) return;

        // Move arrow up/down
        float delta = _profile.speed * Time.unscaledDeltaTime; // unscaled for UI minigame
        _arrowPosition += _direction * delta;

        if (_arrowPosition >= 1f) { _arrowPosition = 1f; _direction = -1f; }
        if (_arrowPosition <= 0f) { _arrowPosition = 0f;_direction = 1f; }

        ApplyArrowUI(_arrowPosition);

        // Optional timeout
        if (_profile.timeLimit > 0f)
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= _profile.timeLimit)
                End(false);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!_running || _profile == null) return;

        // success if arrow is within zone
        float half = _profile.zoneSize * 0.5f;
        float min = Mathf.Clamp01(_profile.zoneCenter - half);
        float max = Mathf.Clamp01(_profile.zoneCenter + half);

        bool success = (_arrowPosition >= min && _arrowPosition <= max);
        Debug.Log("Game Success: " + success);
        End(success);
    }

    private void ApplyZoneUI(ReactionMinigameProfile p)
    {
        float half = p.zoneSize * 0.5f;
        float min = Mathf.Clamp01(p.zoneCenter - half);
        float max = Mathf.Clamp01(p.zoneCenter + half);

        // Fill width; occupy min..max vertically
        _zoneRect.anchorMin = new Vector2(0f, min);
        _zoneRect.anchorMax = new Vector2(1f, max);

        _zoneRect.offsetMin = Vector2.zero;
        _zoneRect.offsetMax = Vector2.zero;
    }

    private void ApplyArrowUI(float normalized)
    {
        // Pointer at center X, at Y = normalized
        _arrowRect.anchorMin = new Vector2(0.5f, normalized);
        _arrowRect.anchorMax = new Vector2(0.5f, normalized);
        _arrowRect.anchoredPosition = Vector2.zero;
    }

}
