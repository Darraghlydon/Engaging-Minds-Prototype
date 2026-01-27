using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float baseIntervalMin = 3f;
    [SerializeField] private float baseIntervalMax = 7f;

    [Tooltip("Higher = more frequent noises. 1 = normal, 2 = twice as often, 0.5 = half as often.")]
    [SerializeField] private float rateMultiplier = 1f;

    private readonly HashSet<Passenger> passengers = new();

    public void Register(Passenger p) => passengers.Add(p);
    public void Unregister(Passenger p) => passengers.Remove(p);

    public void SetRateMultiplier(float m) => rateMultiplier = Mathf.Max(0.01f, m);

    void Start()
    {
        StartCoroutine(NoiseLoop());
    }

    private IEnumerator NoiseLoop()
    {
        while (true)
        {
            float wait = Random.Range(baseIntervalMin, baseIntervalMax);
            wait /= rateMultiplier; // speed up/slow down globally
            yield return new WaitForSeconds(wait);

            TryTriggerNoise();
        }
    }

    private void TryTriggerNoise()
    {
        var eligible = passengers.Where(p => p != null && p.IsEligibleForNoise).ToList();
        if (eligible.Count == 0) return;

        var chosen = eligible[Random.Range(0, eligible.Count)];
        chosen.SetNoisy(true);
    }
}

