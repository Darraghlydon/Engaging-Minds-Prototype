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
    [SerializeField] private float _seatedTimeMin = 6f;
    [SerializeField] private float _seatedTimeMax = 15f;

    private NavMeshAgent _agent;
    private SeatManager _seatManager;

    private Transform _frontExit;
    private Transform _backExit;

    private PassengerSpawner _spawner;

    public Seat seat { get; private set; }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        speechBubble.SetActive(false);
    }

    public void Init(SeatManager manager, Transform frontExit, Transform backExit, PassengerSpawner spawner)
    {
        _seatManager = manager;
        this._frontExit = frontExit;
        this._backExit = backExit;
        this._spawner = spawner;
    }


    public void StartSeated(Seat startSeat)
    {
        seat = startSeat;
        state = State.Seated;

        _agent.enabled = false;
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

        if (!_agent.enabled) _agent.enabled = true;
        _agent.isStopped = false;
        _agent.SetDestination(seat.approachPoint.position);
    }

    void Update()
    {
        if (state == State.WalkingToSeat && _agent.enabled && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            {
                SitDown();
            }
        }
        else if (state == State.Leaving && _agent.enabled && !_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance + 0.2f)
            {
                Destroy(gameObject); // or return to pool
            }
        }
    }


    private void SitDown()
    {
        state = State.Seated;
        _agent.isStopped = true;
        _agent.enabled = false;

        transform.SetPositionAndRotation(seat.sitPoint.position, seat.sitPoint.rotation);
        StartCoroutine(SeatedRoutine());
    }


    private IEnumerator SeatedRoutine()
    {
        while (state == State.Seated)
        {
            yield return new WaitForSeconds(Random.Range(_seatedTimeMin, _seatedTimeMax));
            if (state != State.Seated) yield break;

            if (_spawner != null && _spawner.CanAnyPassengerLeave())
            {
                LeaveViaRandomExit();
                yield break;
            }
        }
    }

    private void LeaveViaRandomExit()
    {

        // Remove from active count immediately
        _spawner?.Unregister(this);
        OnBeginLeaving();

        // Free the seat
        if (_seatManager != null && seat != null)
            _seatManager.ReleaseSeat(seat, this);

        seat = null;
        state = State.Leaving;

        if (!_agent.enabled) _agent.enabled = true;
        _agent.isStopped = false;

        Transform exit = GetRandomExit();
        _agent.SetDestination(exit.position);
    }

    private Transform GetRandomExit()
    {
        return Random.value < 0.5f ? _frontExit : _backExit;
    }
}
