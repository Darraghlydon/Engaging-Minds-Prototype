using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionMinigameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _panelRoot;     // ReactionMinigamePanel
    [SerializeField] private RectTransform _barRect;    // Bar rect
    [SerializeField] private RectTransform _successZoneRect;   // SuccessZone rect
    [SerializeField] private RectTransform _arrowRect; // Arrow rect
    [Header("Settings")]
    [SerializeField] private ReactionMinigameProfile _defaultProfile;
    [SerializeField] private ReactionMinigameProfile _resetProfile;
    [SerializeField] private float _maxSpeed=4;
    [SerializeField] private float _minZone=1;
    [SerializeField] private float _speedPenalty;
    [SerializeField] private float _zonePenalty;

    private Action<bool> onComplete;
    

    private float _arrowPosition;          // arrow normalized position (0..1)
    private float _direction = 1f;   // +1 up, -1 down
    private float _elapsed;
    private bool _running;
    private float _zoneCenter;
    private float _zoneSize;
    private float _speed;
    private float _timeLimit;

    void Awake()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
        _zoneCenter = _defaultProfile.zoneCenter;
        _zoneSize = _defaultProfile.zoneSize;
        _speed = _defaultProfile.speed;
        _timeLimit = _defaultProfile.timeLimit;
    }

    private void Start()
    {
        InputManager.Instance.Actions.ReactionGame.Interact.performed += OnInteractPerformed;
    }


    void OnDestroy()
    {
        if(InputManager.Instance != null)
        {
            InputManager.Instance.Actions.ReactionGame.Interact.performed -= OnInteractPerformed;
        }
        
    }

    public void Show()
    {
        InputManager.Instance.SwitchToReactionGame();
        _arrowPosition = UnityEngine.Random.value;
        _direction = 1f;
        _elapsed = 0f;
        _running = true;

        ApplyZoneUI();
        if (_panelRoot != null) _panelRoot.SetActive(true);

    }

    private void End(bool success)
    {
        if (!_running) return;
        _running = false;
        InputManager.Instance.SwitchToMainGame();
        AdjustSuccessZone(success);
        if (!success)
        {
            Events.IncreaseStress.Publish();
        }
        if (_panelRoot != null) _panelRoot.SetActive(false);

    }

    private void AdjustSuccessZone(bool success)
    {
        if (success)
        {
        }
        else
        {
            _zoneSize = Mathf.Max(_minZone, _zoneSize - _zonePenalty);
            _speed = Mathf.Min(_maxSpeed, _speed + _speedPenalty);
        }
    }

    void Update()
    {
        if (!_running || GamePause.Mode != PauseMode.WorldOnly) return;

        // Move arrow up/down
        float delta = _speed * Time.unscaledDeltaTime; // unscaled for UI minigame
        _arrowPosition += _direction * delta;

        if (_arrowPosition >= 1f) { _arrowPosition = 1f; _direction = -1f; }
        if (_arrowPosition <= 0f) { _arrowPosition = 0f;_direction = 1f; }

        ApplyArrowUI(_arrowPosition);

        // Optional timeout
        if (_timeLimit > 0f)
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= _timeLimit)
                End(false);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!_running || GamePause.Mode != PauseMode.WorldOnly) return;

        // success if arrow is within zone
        float half = _zoneSize * 0.5f;
        float min = Mathf.Clamp01(_zoneCenter - half);
        float max = Mathf.Clamp01(_zoneCenter + half);

        bool success = (_arrowPosition >= min && _arrowPosition <= max);
        Debug.Log("Game Success: " + success);
        End(success);
    }

    private void ApplyZoneUI()
    {
        float half = _zoneSize * 0.5f;
        float min = Mathf.Clamp01(_zoneCenter - half);
        float max = Mathf.Clamp01(_zoneCenter + half);

        // Fill width; occupy min..max vertically
        _successZoneRect.anchorMin = new Vector2(0f, min);
        _successZoneRect.anchorMax = new Vector2(1f, max);

        _successZoneRect.offsetMin = Vector2.zero;
        _successZoneRect.offsetMax = Vector2.zero;
    }

    private void ApplyArrowUI(float normalized)
    {
        // Pointer at center X, at Y = normalized
        _arrowRect.anchorMin = new Vector2(0.5f, normalized);
        _arrowRect.anchorMax = new Vector2(0.5f, normalized);
        _arrowRect.anchoredPosition = Vector2.zero;
    }

}
