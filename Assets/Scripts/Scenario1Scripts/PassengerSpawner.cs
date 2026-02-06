using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassengerCharacterDefinition
{
    public string characterId;

    [Header("Prefab")]
    public Passenger passengerPrefab;
}

public class PassengerSpawner : MonoBehaviour
{
    [Header("Minimum Passengers")]
    [SerializeField] private int _minPassengersToKeep = 2;

    [Header("References")]
    [SerializeField] private SeatManager _seatManager;
    [SerializeField] private NoiseManager _noiseManager;

    [Header("Spawn / Exit Points")]
    [SerializeField] private Transform _frontSpawn;
    [SerializeField] private Transform _backSpawn;
    [SerializeField] private Transform _frontExit;
    [SerializeField] private Transform _backExit;

    [Header("Starting Setup")]
    [SerializeField] private int startSeatedCount = 2;

    [Header("Spawning")]
    [SerializeField] private float _spawnIntervalMin = 30f;
    [SerializeField] private float _spawnIntervalMax = 60f;

    [Header("Characters")]
    [SerializeField] private List<PassengerCharacterDefinition> _characters;

    private List<PassengerCharacterDefinition> _unusedCharacters;
    private List<PassengerCharacterDefinition> _usedCharacters;

    private readonly HashSet<Passenger> activePassengers = new();

    public int ActiveCount => activePassengers.Count;

    public bool CanAnyPassengerLeave()
    {
        return activePassengers.Count > _minPassengersToKeep;
    }

    public void Register(Passenger p) => activePassengers.Add(p);
    public void Unregister(Passenger p) => activePassengers.Remove(p);

    private void Start()
    {
        _unusedCharacters = new List<PassengerCharacterDefinition>(_characters);
        _usedCharacters = new List<PassengerCharacterDefinition>();

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

    // ----------------------------------------------------
    // WALK-IN SPAWNING (only characters that can walk)
    // ----------------------------------------------------
    private void TrySpawnOne()
    {
        if (_seatManager.FreeSeatCount <= 0)
            return;

        var character = GetNextCharacter();
        if (character == null)
            return;

        Transform spawn = Random.value < 0.5f ? _frontSpawn : _backSpawn;

        var p = Instantiate(character.passengerPrefab,
                             spawn.position,
                             spawn.rotation);

        p.Init(_seatManager, _frontExit, _backExit, this);

        // If this passenger cannot walk, discard and try again later
        if (!p.CanWalk)
        {
            Destroy(p.gameObject);
            return;
        }

        Register(p);
        _noiseManager.Register(p);

        if (!_seatManager.TryClaimRandomSeat(p, out var seat))
        {
            CleanupFailedSpawn(p);
            return;
        }

        p.WalkToSeat(seat);
    }


    // ----------------------------------------------------
    // STARTING SEATED PASSENGERS (all characters allowed)
    // ----------------------------------------------------
    private void SpawnStartingSeatedPassengers()
    {
        int count = Mathf.Clamp(startSeatedCount, 0, _seatManager.FreeSeatCount);

        for (int i = 0; i < count; i++)
        {
            var character = GetNextCharacter();

            var p = Instantiate(character.passengerPrefab);

            p.Init(_seatManager, _frontExit, _backExit, this);

            Register(p);
            _noiseManager.Register(p);

            if (!_seatManager.TryClaimRandomSeat(p, out var seat))
            {
                CleanupFailedSpawn(p);
                return;
            }

            p.StartSeated(seat);
        }
    }


    // ----------------------------------------------------
    // CHARACTER SELECTION (shuffle-bag)
    // ----------------------------------------------------
    private PassengerCharacterDefinition GetNextCharacter()
    {
        if (_unusedCharacters.Count == 0)
        {
            _unusedCharacters.AddRange(_usedCharacters);
            _usedCharacters.Clear();
        }

        int index = Random.Range(0, _unusedCharacters.Count);
        var chosen = _unusedCharacters[index];

        _unusedCharacters.RemoveAt(index);
        _usedCharacters.Add(chosen);

        return chosen;
    }


    // ----------------------------------------------------
    // CLEANUP
    // ----------------------------------------------------
    private void CleanupFailedSpawn(Passenger p)
    {
        if (p == null) return;

        Unregister(p);
        _noiseManager.Unregister(p);
        Destroy(p.gameObject);
    }
}
