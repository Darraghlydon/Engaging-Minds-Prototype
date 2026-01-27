using UnityEngine;
using UnityEngine.AI;

public class Passenger : MonoBehaviour
{
    public enum State { Entering, WalkingToSeat, Seated, Leaving }
    public State state;

    private NavMeshAgent agent;
    private SeatManager seatManager;

    public Seat seat { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(SeatManager manager)
    {
        seatManager = manager;
    }

    public void StartSeated(Seat startSeat)
    {
        seat = startSeat;
        state = State.Seated;

        agent.enabled = false; // seated = no nav movement
        transform.SetPositionAndRotation(seat.sitPoint.position, seat.sitPoint.rotation);
    }

    public void WalkToSeat(Seat targetSeat)
    {
        seat = targetSeat;
        state = State.WalkingToSeat;

        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(seat.approachPoint.position);
    }

    void Update()
    {
        if (state == State.WalkingToSeat && agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                SitDown();
            }
        }
    }

    private void SitDown()
    {
        state = State.Seated;
        agent.isStopped = true;
        agent.enabled = false;

        transform.SetPositionAndRotation(seat.sitPoint.position, seat.sitPoint.rotation);
        // Optional: play sit anim here
    }

    public void LeaveAndExit(Transform exitPoint)
    {
        // Release seat as soon as they commit to leaving (prevents blocking spawns)
        if (seatManager != null && seat != null)
            seatManager.ReleaseSeat(seat, this);

        seat = null;
        state = State.Leaving;

        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(exitPoint.position);
    }
}

