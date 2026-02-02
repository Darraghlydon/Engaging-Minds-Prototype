using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeatManager : MonoBehaviour
{
    [SerializeField] private Transform seatsRoot; // parent object containing seats

    private List<Seat> seats = new();

    public IEnumerable<Seat> AllSeats => seats;
    public int FreeSeatCount => seats.Count(s => !s.IsOccupied);

    void Awake()
    {
        CacheSeats();
    }

    private void CacheSeats()
    {
        seats.Clear();

        if (seatsRoot == null)
        {
            Debug.LogWarning("SeatManager: SeatsRoot not assigned.");
            return;
        }

        seats.AddRange(seatsRoot.GetComponentsInChildren<Seat>(includeInactive: false));

        if (seats.Count == 0)
            Debug.LogWarning("SeatManager: No Seat components found under SeatsRoot.");
    }

    public bool TryClaimRandomSeat(Passenger p, out Seat seat)
    {
        var free = seats.Where(s => !s.IsOccupied).ToList();
        if (free.Count == 0)
        {
            seat = null;
            return false;
        }

        seat = free[Random.Range(0, free.Count)];
        return seat.TryClaim(p);
    }

    public void ReleaseSeat(Seat seat, Passenger p)
    {
        if (seat != null) seat.Release(p);
    }
}
