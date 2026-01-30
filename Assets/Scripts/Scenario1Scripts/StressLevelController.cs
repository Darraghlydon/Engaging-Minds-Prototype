using UnityEngine;
using System.Collections;

public class StressLevelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform _barRect;   // background bar
    [SerializeField] private RectTransform _fillRect;  // fill image

    [Header("Settings")]
    [SerializeField] private int _maxStress = 10;
    [SerializeField] private float _smoothSpeed = 10f; // higher = snappier

    public int CurrentStress { get; private set; }

    private float _displayFill; 
    private float _targetFill; 

    void Start()
    {
        SetStress(0);
        _displayFill = _targetFill;
        ApplyFill(_displayFill);
    }

    void Update()
    {

    }

    private void OnEnable()
    {
        Events.MiniGameSuccess.Subscribe(HandleMiniGameSuccessEvent);
    }

    private void OnDisable()
    {
        Events.MiniGameSuccess.Unsubscribe(HandleMiniGameSuccessEvent);
    }

    void HandleMiniGameSuccessEvent(bool success)
    {
        if (!success)
        {
            AddStress(1);
        }
    }

    public void AddStress(int amount = 1)
    {
        SetStress(CurrentStress + amount);
    }

    public void ReduceStress(int amount = 1)
    {
        SetStress(CurrentStress - amount);
    }

    public void SetStress(int value)
    {
        CurrentStress = Mathf.Clamp(value, 0, _maxStress);
        _targetFill = (_maxStress <= 0) ? 0f : (float)CurrentStress / _maxStress;
        StartCoroutine(AnimateFill(_displayFill, _targetFill));
    }

    private void ApplyFill(float normalized)
    {
        // Fill grows left -> right
        _fillRect.anchorMin = new Vector2(0f, 0f);
        _fillRect.anchorMax = new Vector2(normalized, 1f);
        _fillRect.offsetMin = Vector2.zero;
        _fillRect.offsetMax = Vector2.zero;
    }

    private IEnumerator AnimateFill(float from, float to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * _smoothSpeed;

            _displayFill = Mathf.Lerp(from, to, t);
            ApplyFill(_displayFill);

            yield return null;
        }

        _displayFill = to;
        ApplyFill(_displayFill);
    }

}
