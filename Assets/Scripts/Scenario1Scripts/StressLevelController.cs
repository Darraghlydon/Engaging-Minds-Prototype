using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StressLevelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform _barRect;   // background bar
    [SerializeField] private RectTransform _fillRect;  // fill image

    [Header("Settings")]
    [SerializeField] private int _maxStress = 10;
    [SerializeField] private float _smoothSpeed = 10f; // higher = snappier

    [Header("Volume")]
    [SerializeField] private Volume _postProcessVolume;
    [SerializeField] private bool _enableVignette=true;
    [SerializeField, Range(0f, 1f)] private float _maxVignetteIntensity = 0.8f;

    private Vignette _vignette;

    public int CurrentStress { get; private set; }

    private float _displayFill; 
    private float _targetFill; 

    void Start()
    {
        SetStress(0);
        _displayFill = _targetFill;
        ApplyFill(_displayFill);
    }

    void Awake()
    {
        if (_postProcessVolume.profile.TryGet(out _vignette))
        {
            _vignette.intensity.Override(0f);
        }
        else
        {
            Debug.LogWarning("No Vignette found in Volume Profile");
        }
    }

    private void OnEnable()
    {
        Events.IncreaseStress.Subscribe(IncreaseStress);
    }

    private void OnDisable()
    {
        Events.IncreaseStress.Unsubscribe(IncreaseStress);
    }

    void IncreaseStress()
    {
        AddStress(1);
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
        float normalized = (_maxStress <= 0) ? 0f : (float)CurrentStress / _maxStress;
        StartCoroutine(AnimateFill(_displayFill, _targetFill));
        if (CurrentStress>=_maxStress)
        {
            Events.MaxStressReached.Publish();
        }
    }

    private void ApplyFill(float normalized)
    {
        // Fill grows left -> right
        _fillRect.anchorMin = new Vector2(0f, 0f);
        _fillRect.anchorMax = new Vector2(normalized, 1f);
        _fillRect.offsetMin = Vector2.zero;
        _fillRect.offsetMax = Vector2.zero;

        if (_vignette != null && _enableVignette)
            _vignette.intensity.Override(normalized * _maxVignetteIntensity);
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
