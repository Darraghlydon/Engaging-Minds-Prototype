using System.Collections;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
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
    [SerializeField] private float spawnIntervalMin = 3f;
    [SerializeField] private float spawnIntervalMax = 8f;

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
        p.Init(seatManager, frontExit, backExit);

        if (!seatManager.TryClaimRandomSeat(p, out var seat))
        {
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
            p.Init(seatManager, frontExit, backExit);

            if (!seatManager.TryClaimRandomSeat(p, out var seat))
            {
                Destroy(p.gameObject);
                return;
            }

            p.StartSeated(seat);
        }
    }
}
