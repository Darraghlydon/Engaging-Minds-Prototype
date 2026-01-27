using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeatManager : MonoBehaviour
{
    [SerializeField] private List<Seat> seats = new();

    public int FreeSeatCount => seats.Count(s => !s.IsOccupied);

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

    public IEnumerable<Seat> AllSeats => seats;
}
