using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMeshAgentController : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        _navMeshAgent.nextPosition = transform.position;
    }
}
