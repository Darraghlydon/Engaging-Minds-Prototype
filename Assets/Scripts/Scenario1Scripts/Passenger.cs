using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Passenger : MonoBehaviour
{
    public enum State { WalkingToSeat, Seated, Leaving }
    public State state;

    [Header("Noise Indicator")]
    [SerializeField] private GameObject speechBubble; // assign in inspector

    public bool IsNoisy { get; private set; }

    public bool IsEligibleForNoise =>
        state == State.Seated && !IsNoisy;   // simple rule for now

    [Header("Leave Timing")]
    [SerializeField] private float seatedTimeMin = 6f;
    [SerializeField] private float seatedTimeMax = 15f;

    private NavMeshAgent agent;
    private SeatManager seatManager;

    private Transform frontExit;
    private Transform backExit;

    private PassengerSpawner spawner;

    public Seat seat { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speechBubble.SetActive(false);
    }



    public void Init(SeatManager manager, Transform frontExit, Transform backExit, PassengerSpawner spawner)
    {
        seatManager = manager;
        this.frontExit = frontExit;
        this.backExit = backExit;
        this.spawner = spawner;
    }


    public void StartSeated(Seat startSeat)
    {
        seat = startSeat;
        state = State.Seated;

        agent.enabled = false;
        transform.SetPositionAndRotation(seat.sitPoint.position, seat.sitPoint.rotation);

        StartCoroutine(SeatedRoutine());
    }

    public void SetNoisy(bool noisy)
    {
        IsNoisy = noisy;
        if (speechBubble != null)
            speechBubble.SetActive(noisy);
    }

    public void Silence()
    {
        SetNoisy(false);
        // Later: trigger your minigame here or via a manager
    }

    public void OnBeginLeaving()
    {
        SetNoisy(false); 
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
        else if (state == State.Leaving && agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                Destroy(gameObject); // or return to pool
            }
        }
    }


    private void SitDown()
    {
        state = State.Seated;
        agent.isStopped = true;
        agent.enabled = false;

        transform.SetPositionAndRotation(seat.sitPoint.position, seat.sitPoint.rotation);
        StartCoroutine(SeatedRoutine());
    }


    private IEnumerator SeatedRoutine()
    {
        while (state == State.Seated)
        {
            yield return new WaitForSeconds(Random.Range(seatedTimeMin, seatedTimeMax));
            if (state != State.Seated) yield break;

            if (spawner != null && spawner.CanAnyPassengerLeave())
            {
                LeaveViaRandomExit();
                yield break;
            }
        }
    }

    private void LeaveViaRandomExit()
    {

        // Remove from active count immediately
        spawner?.Unregister(this);
        OnBeginLeaving();

        // Free the seat
        if (seatManager != null && seat != null)
            seatManager.ReleaseSeat(seat, this);

        seat = null;
        state = State.Leaving;

        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = false;

        Transform exit = GetRandomExit();
        agent.SetDestination(exit.position);
    }

    private Transform GetRandomExit()
    {
        return Random.value < 0.5f ? frontExit : backExit;
    }
}
