using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("Minimum passengers that must remain")]
    [SerializeField] private int _minPassengersToKeep = 2;
    [SerializeField] private NoiseManager _noiseManager;

    private readonly HashSet<Passenger> activePassengers = new();

    public bool CanAnyPassengerLeave()
    {
        return activePassengers.Count > _minPassengersToKeep;
    }

    public void Register(Passenger p) => activePassengers.Add(p);
    public void Unregister(Passenger p) => activePassengers.Remove(p);

    public int ActiveCount => activePassengers.Count;

    [Header("References")]
    [SerializeField] private SeatManager _seatManager;
    [SerializeField] private Passenger _passengerPrefab;

    [Header("Spawn/Exit Points")]
    [SerializeField] private Transform _frontSpawn;
    [SerializeField] private Transform _backSpawn;
    [SerializeField] private Transform _frontExit;
    [SerializeField] private Transform _backExit;

    [Header("Starting Setup")]
    [SerializeField] private int startSeatedCount = 2;

    [Header("Spawning")]
    [SerializeField] private float _spawnIntervalMin = 30f;
    [SerializeField] private float _spawnIntervalMax = 60f;

    void Start()
    {
        SpawnStartingSeatedPassengers();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_spawnIntervalMin, _spawnIntervalMax));
            TrySpawnOne();
        }
    }

    private void TrySpawnOne()
    {
        if (_seatManager.FreeSeatCount <= 0) return;

        Transform chosenSpawn = Random.value < 0.5f ? _frontSpawn : _backSpawn;

        var p = Instantiate(_passengerPrefab, chosenSpawn.position, chosenSpawn.rotation);
        p.Init(_seatManager, _frontExit, _backExit, this);
        Register(p);
        _noiseManager.Register(p);

        if (!_seatManager.TryClaimRandomSeat(p, out var seat))
        {
            Unregister(p);
            _noiseManager.Unregister(p);
            Destroy(p.gameObject);
            return;
        }

        p.WalkToSeat(seat);
    }

    private void SpawnStartingSeatedPassengers()
    {
        int count = Mathf.Clamp(startSeatedCount, 0, _seatManager.FreeSeatCount);

        for (int i = 0; i < count; i++)
        {
            var p = Instantiate(_passengerPrefab);
            p.Init(_seatManager, _frontExit, _backExit, this);

            Register(p);
            _noiseManager.Register(p);

            if (!_seatManager.TryClaimRandomSeat(p, out var seat))
            {
                Unregister(p);
                _noiseManager.Unregister(p);
                Destroy(p.gameObject);
                return;
            }

            p.StartSeated(seat);
        }
    }
}
