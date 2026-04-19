using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CrawlerSurfaceAligner : MonoBehaviour
{
    [SerializeField] private float _rotationSpeedMultiplier = 3f;
    [SerializeField] private float _linkSpeedMultiplier     = 1f;
    [SerializeField] private bool _showDebug = true;

    private NavMeshAgent _agent;
    private bool       _traversingLink;
    private Vector3    _linkStart;
    private Vector3    _linkEnd;
    private float      _linkProgress;
    private float      _rotationProgress;
    private float      _linkLength;
    private Quaternion _linkStartRot;
    private Quaternion _linkEndRot;

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; if (!_traversingLink) SetAgentRotation(value); }
    }

    private void Awake() => _agent = GetComponent<NavMeshAgent>();

    private void Start()
    {
        _agent.autoTraverseOffMeshLink = false;
        SetAgentRotation(true);
    }

    private void Update()
    {
        if (!_isActive) return;
        if (_traversingLink) { TickLinkTraversal(); return; }
        if (_agent.isOnOffMeshLink) { BeginLinkTraversal(); }
    }

    private void BeginLinkTraversal()
    {
        OffMeshLinkData link = _agent.currentOffMeshLinkData;
        _linkStart = transform.position;
        _linkEnd = link.endPos;
        _linkLength = Vector3.Distance(_linkStart, _linkEnd);
        _linkProgress = 0f;
        _rotationProgress = 0f;
        _traversingLink = true;
        _agent.isStopped = true;

        SetAgentRotation(false);

        Vector3 moveDir = (_linkEnd - _linkStart).normalized;
        
        transform.rotation = Quaternion.LookRotation(moveDir, transform.up);
        _linkStartRot = transform.rotation;

        Vector3 targetNormal = GetTargetNormal(_linkEnd, moveDir);

        Vector3 forwardOnSurface = Vector3.ProjectOnPlane(moveDir, targetNormal).normalized;
        if (forwardOnSurface.sqrMagnitude < 0.01f) forwardOnSurface = moveDir;

        _linkEndRot = Quaternion.LookRotation(forwardOnSurface, targetNormal);
    }

    private Vector3 GetTargetNormal(Vector3 endPos, Vector3 moveDir)
    {
        Vector3[] searchOffsets = {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        foreach (Vector3 offset in searchOffsets)
        {
            if (Physics.Raycast(endPos + offset * 0.5f, -offset, out RaycastHit hit, 1f))
            {
                if (Vector3.Distance(hit.point, endPos) < 0.3f)
                {
                    return hit.normal;
                }
            }
        }

        float absX = Mathf.Abs(endPos.x);
        float absY = Mathf.Abs(endPos.y);
        float absZ = Mathf.Abs(endPos.z);

        if (absY > absX && absY > absZ) return endPos.y > 0 ? Vector3.down : Vector3.up;
        if (absX > absY && absX > absZ) return endPos.x > 0 ? Vector3.left : Vector3.right;
        return endPos.z > 0 ? Vector3.back : Vector3.forward;
    }

    private void TickLinkTraversal()
    {
        if (_linkLength < 0.001f) { FinishLinkTraversal(); return; }

        float moveStep = (_agent.speed * Time.deltaTime) / _linkLength;
        _linkProgress = Mathf.MoveTowards(_linkProgress, 1f, moveStep * _linkSpeedMultiplier);
        
        _rotationProgress = Mathf.MoveTowards(_rotationProgress, 1f, moveStep * _rotationSpeedMultiplier);

        transform.position = Vector3.Lerp(_linkStart, _linkEnd, _linkProgress);
        transform.rotation = Quaternion.Slerp(_linkStartRot, _linkEndRot, _rotationProgress);

        if (_linkProgress >= 1f) FinishLinkTraversal();
    }

    private void FinishLinkTraversal()
    {
        transform.SetPositionAndRotation(_linkEnd, _linkEndRot);
        _agent.Warp(_linkEnd);
        _agent.CompleteOffMeshLink();
        
        StartCoroutine(RestoreRotationRoutine());
    }

    private IEnumerator RestoreRotationRoutine()
    {
        yield return null;
        _traversingLink = false;
        _agent.isStopped = false;
        SetAgentRotation(_isActive);
    }

    private void SetAgentRotation(bool enabled)
    {
        _agent.updateRotation = enabled;
        _agent.updateUpAxis = enabled; 
    }

    private void OnDrawGizmos()
    {
        if (!_traversingLink || !_showDebug) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_linkStart, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_linkEnd, 0.1f);
        Gizmos.DrawLine(_linkStart, _linkEnd);
    }
}