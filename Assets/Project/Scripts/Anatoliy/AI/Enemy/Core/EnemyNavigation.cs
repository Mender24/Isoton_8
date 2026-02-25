using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigation : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _runSpeed  = 5f;

    [Header("Agent Settings")]
    [SerializeField] private float _stoppingDistance = 0.5f;
    [SerializeField] private float _rotationSpeed    = 10f;

    public float WalkSpeed      => _walkSpeed;
    public float RunSpeed       => _runSpeed;
    public float RotationSpeed  => _rotationSpeed;
    public NavMeshAgent Agent   => _agent;
    public float CurrentSpeed   => _agent != null ? _agent.velocity.magnitude : 0f;

    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = _stoppingDistance;
    }

    public void MoveTo(Vector3 destination, bool run = true)
    {
        if (!_agent.isOnNavMesh) return;
        _agent.speed = run ? _runSpeed : _walkSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (_agent.isOnNavMesh)
            _agent.isStopped = true;
    }

    public void Resume()
    {
        if (_agent.isOnNavMesh)
            _agent.isStopped = false;
    }

    public void ResetPath()
    {
        if (_agent.isOnNavMesh)
            _agent.ResetPath();
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
    }

    public void SetStoppingDistance(float dist)
    {
        _agent.stoppingDistance = dist;
    }

    public bool HasReachedDestination()
    {
        if (_agent == null) return false;
        if (_agent.pathPending) return false;
        if (_agent.remainingDistance > _agent.stoppingDistance) return false;
        return !_agent.hasPath || _agent.velocity.sqrMagnitude == 0f;
    }

    public bool TryGetRandomNavPoint(Vector3 origin, float radius, out Vector3 result)
    {
        result = Vector3.zero;
        Vector3 randomDir = Random.insideUnitSphere * radius + origin;

        for (int i = 0; i < 10; i++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (_agent.CalculatePath(hit.position, path))
                {
                    result = hit.position;
                    return true;
                }
            }
            randomDir = Random.insideUnitSphere * radius + origin;
        }

        return false;
    }

    public void DisableAgent()
    {
        if (_agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
        _agent.enabled = false;
    }

    public void EnableAgent()
    {
        _agent.enabled = true;
        _agent.isStopped = false;
    }
}
