using System.Linq;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [SerializeField] private SeatManager seatManager;
    [SerializeField] private Passenger passengerPrefab;

    [Header("Start Setup")]
    [SerializeField] private int startSeatedCount = 2;

    void Start()
    {
        SpawnStartingSeatedPassengers();
    }

    private void SpawnStartingSeatedPassengers()
    {
        var freeSeats = seatManager.AllSeats.Where(s => !s.IsOccupied).ToList();
        int count = Mathf.Clamp(startSeatedCount, 0, freeSeats.Count);

        for (int i = 0; i < count; i++)
        {
            var seat = freeSeats[Random.Range(0, freeSeats.Count)];
            freeSeats.Remove(seat);

            var p = Instantiate(passengerPrefab);
            p.Init(seatManager);

            // Claim then snap seated
            seat.TryClaim(p);
            p.StartSeated(seat);
        }
    }
}

