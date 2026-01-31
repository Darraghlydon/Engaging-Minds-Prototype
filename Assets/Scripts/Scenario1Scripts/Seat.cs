using UnityEngine;

public class Seat : MonoBehaviour
{
    public Transform approachPoint;
    public Transform sitPoint;

    public bool IsOccupied => occupant != null;
    public Passenger occupant;

    public bool TryClaim(Passenger p)
    {
        if (IsOccupied) return false;
        occupant = p;
        return true;
    }

    public void Release(Passenger p)
    {
        if (occupant == p) occupant = null;
    }
}
