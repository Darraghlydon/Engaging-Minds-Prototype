using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _baseIntervalMin = 3f;
    [SerializeField] private float _baseIntervalMax = 7f;

    [Tooltip("Higher = more frequent noises. 1 = normal, 2 = twice as often, 0.5 = half as often.")]
    [SerializeField] private float _rateMultiplier = 1f;
    [SerializeField] private float _rateMultiplierIncrement = 0.2f;
    private float _rateMultiplierDefault;

    private readonly HashSet<Passenger> _passengers = new();

    public void Register(Passenger p) => _passengers.Add(p);
    public void Unregister(Passenger p) => _passengers.Remove(p);

    public void SetRateMultiplier(float m) => _rateMultiplier = Mathf.Max(0.01f, m);

    void Start()
    {
        _rateMultiplierDefault = _rateMultiplier;

        StartCoroutine(NoiseLoop());
    }

    private IEnumerator NoiseLoop()
    {
        while (true)
        {
            float wait = Random.Range(_baseIntervalMin, _baseIntervalMax);
            wait /= _rateMultiplier; // speed up/slow down globally
            yield return new WaitForSeconds(wait);

            TryTriggerNoise();
        }
    }

    private void OnEnable()
    {
        Events.IncreaseStress.Subscribe(IncreaseRateMultiplier);
        Events.ReduceStress.Subscribe(ResetRateMultiplier);
    }

    private void OnDisable()
    {
        Events.IncreaseStress.Unsubscribe(IncreaseRateMultiplier);
        Events.ReduceStress.Unsubscribe(ResetRateMultiplier);
    }

    void IncreaseRateMultiplier()
    {
        _rateMultiplier = _rateMultiplier + _rateMultiplierIncrement;
        SetRateMultiplier(_rateMultiplier);
    }

    void ResetRateMultiplier()
    {
        _rateMultiplier=_rateMultiplierDefault; 
    }

    private void TryTriggerNoise()
    {
        var eligible = _passengers.Where(p => p != null && p.IsEligibleForNoise).ToList();
        if (eligible.Count == 0) return;

        var chosen = eligible[Random.Range(0, eligible.Count)];
        chosen.SetNoisy(true);
    }
}

