using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("Minimum passengers that must remain")]
    [SerializeField] private int minPassengersToKeep = 2;
    [SerializeField] private NoiseManager noiseManager;

    private readonly HashSet<Passenger> activePassengers = new();

    public bool CanAnyPassengerLeave()
    {
        return activePassengers.Count > minPassengersToKeep;
    }

    public void Register(Passenger p) => activePassengers.Add(p);
    public void Unregister(Passenger p) => activePassengers.Remove(p);

    public int ActiveCount => activePassengers.Count;

    [Header("References")]
    [SerializeField] private SeatManager seatManager;
    [SerializeField] private Passenger passengerPrefab;

    [Header("Spawn/Exit Points")]
    [SerializeField] private Transform frontSpawn;
    [SerializeField] private Transform backSpawn;
    [SerializeField] private Transform frontExit;
    [SerializeField] private Transform backExit;

    [Header("Starting Setup")]
    [SerializeField] private int startSeatedCount = 2;

    [Header("Spawning")]
    [SerializeField] private float spawnIntervalMin = 30f;
    [SerializeField] private float spawnIntervalMax = 60f;

    void Start()
    {
        SpawnStartingSeatedPassengers();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            TrySpawnOne();
        }
    }

    private void TrySpawnOne()
    {
        if (seatManager.FreeSeatCount <= 0) return;

        Transform chosenSpawn = Random.value < 0.5f ? frontSpawn : backSpawn;

        var p = Instantiate(passengerPrefab, chosenSpawn.position, chosenSpawn.rotation);
        p.Init(seatManager, frontExit, backExit, this);
        Register(p);
        noiseManager.Register(p);

        if (!seatManager.TryClaimRandomSeat(p, out var seat))
        {
            Unregister(p);
            noiseManager.Unregister(p);
            Destroy(p.gameObject);
            return;
        }

        p.WalkToSeat(seat);
    }

    private void SpawnStartingSeatedPassengers()
    {
        int count = Mathf.Clamp(startSeatedCount, 0, seatManager.FreeSeatCount);

        for (int i = 0; i < count; i++)
        {
            var p = Instantiate(passengerPrefab);
            p.Init(seatManager, frontExit, backExit, this);

            Register(p);
            noiseManager.Register(p);

            if (!seatManager.TryClaimRandomSeat(p, out var seat))
            {
                Unregister(p);
                noiseManager.Unregister(p);
                Destroy(p.gameObject);
                return;
            }

            p.StartSeated(seat);
        }
    }
}
