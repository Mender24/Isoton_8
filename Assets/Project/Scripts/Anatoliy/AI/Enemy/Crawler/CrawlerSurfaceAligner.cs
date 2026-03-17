using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Выравнивает ориентацию объекта по нормали поверхности.
/// Во время traversal NavMeshLink непрерывно зондирует поверхность в текущей позиции,
/// используя фиксированный эталон _linkStartNormal чтобы всегда предпочитать
/// поверхность назначения.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CrawlerSurfaceAligner : MonoBehaviour
{
    [Header("Surface Alignment")]
    [SerializeField] private float     _alignSpeed       = 6f;
    [SerializeField] private float     _surfaceCheckDist = 0.4f;
    [SerializeField] private float     _sphereRadius     = 0.1f;
    [SerializeField] private LayerMask _surfaceLayer;

    [Header("Facing")]
    [SerializeField] private float _faceSpeed         = 10f;
    [SerializeField] private float _velocityThreshold = 0.2f;


    private NavMeshAgent _agent;
    private Vector3      _smoothUp;

    private bool    _traversingLink;
    private Vector3 _linkStart;
    private Vector3 _linkEnd;
    private float   _linkProgress;
    private float   _linkLength;
    private Vector3 _linkStartNormal;

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (!_isActive && value)
                SnapNormalToSurface();
            _isActive = value;
        }
    }

    private static readonly Vector3[] _probeDirections =
    {
        Vector3.down, Vector3.up,
        Vector3.left, Vector3.right,
        Vector3.forward, Vector3.back,
    };

    private void Awake() => _agent = GetComponent<NavMeshAgent>();

    private void Start()
    {
        _smoothUp = transform.up;

        _agent.baseOffset              = 0f;
        _agent.updateRotation          = false;
        _agent.updateUpAxis            = false;
        _agent.autoTraverseOffMeshLink = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        if (_traversingLink)        { TickLinkTraversal(); return; }
        if (_agent.isOnOffMeshLink) { BeginLinkTraversal(); return; }

        UpdateNormalWalk();
    }

    private void UpdateNormalWalk()
    {
        _smoothUp = Vector3.Slerp(_smoothUp, GetSurfaceNormal(), _alignSpeed * Time.deltaTime);

        Vector3 velocity = _agent.velocity;
        Vector3 forward  = velocity.sqrMagnitude > _velocityThreshold * _velocityThreshold
            ? Vector3.ProjectOnPlane(velocity.normalized, _smoothUp)
            : Vector3.ProjectOnPlane(transform.forward,   _smoothUp);

        if (forward.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(forward.normalized, _smoothUp),
            _faceSpeed * Time.deltaTime);
    }

    private void BeginLinkTraversal()
    {
        OffMeshLinkData link = _agent.currentOffMeshLinkData;
        _linkStart = transform.position;
        _linkEnd   = link.endPos;
        _linkLength   = Vector3.Distance(_linkStart, _linkEnd);
        _linkProgress = 0f;
        _traversingLink  = true;
        _agent.isStopped = true;

        _linkStartNormal = _smoothUp;
    }

    private void TickLinkTraversal()
    {
        if (_linkLength < 0.001f) { FinishLinkTraversal(); return; }

        float step = _agent.speed * Time.deltaTime / _linkLength;
        _linkProgress = Mathf.MoveTowards(_linkProgress, 1f, step);

        transform.position = Vector3.Lerp(_linkStart, _linkEnd, _linkProgress);

        Vector3 destNormal = GetNormalMostDifferentFrom(_linkStartNormal);
        _smoothUp = Vector3.Slerp(_smoothUp, destNormal, _alignSpeed * Time.deltaTime);

        Vector3 linkDir = (_linkEnd - _linkStart).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(linkDir, _smoothUp);
        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(forward.normalized, _smoothUp),
                _faceSpeed * Time.deltaTime);
        }

        if (_linkProgress >= 1f)
            FinishLinkTraversal();
    }

    private void FinishLinkTraversal()
    {
        _traversingLink  = false;
        _agent.isStopped = false;
        _agent.CompleteOffMeshLink();
    }

    private Vector3 GetSurfaceNormal()
    {
        if (Physics.SphereCast(transform.position, _sphereRadius, -transform.up,
                out RaycastHit hit, _surfaceCheckDist, _surfaceLayer))
            return hit.normal;

        return GetNearestNormal();
    }

    private Vector3 GetNearestNormal()
    {
        float   bestDist   = float.MaxValue;
        Vector3 bestNormal = _smoothUp;

        foreach (Vector3 dir in _probeDirections)
        {
            Vector3 origin = transform.position + (-dir) * 0.15f;
            if (Physics.SphereCast(origin, _sphereRadius * 0.5f, dir,
                    out RaycastHit h, _surfaceCheckDist * 3f, _surfaceLayer)
                && h.distance < bestDist)
            {
                bestDist   = h.distance;
                bestNormal = h.normal;
            }
        }
        return bestNormal;
    }

    private Vector3 GetNormalMostDifferentFrom(Vector3 reference)
    {
        float   bestScore  = float.MinValue;
        Vector3 bestNormal = _smoothUp;

        foreach (Vector3 dir in _probeDirections)
        {
            Vector3 origin = transform.position + (-dir) * 0.15f;
            if (Physics.SphereCast(origin, _sphereRadius * 0.5f, dir,
                    out RaycastHit h, _surfaceCheckDist * 3f, _surfaceLayer))
            {
                float score = 1f - Vector3.Dot(h.normal, reference);
                if (score > bestScore)
                {
                    bestScore  = score;
                    bestNormal = h.normal;
                }
            }
        }
        return bestNormal;
    }

    private void SnapNormalToSurface()
    {
        _smoothUp = GetSurfaceNormal();
    }
}
