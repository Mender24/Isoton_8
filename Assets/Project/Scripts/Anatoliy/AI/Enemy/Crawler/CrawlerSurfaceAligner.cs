using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CrawlerSurfaceAligner : MonoBehaviour
{
    [SerializeField] private float _rotationSpeedMultiplier = 1f;
    [SerializeField] private float _linkSpeedMultiplier     = 1f;

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
        set
        {
            _isActive = value;
            if (!_traversingLink)
                SetAgentRotation(value);
        }
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

        if (_traversingLink)        { TickLinkTraversal(); return; }
        if (_agent.isOnOffMeshLink) { BeginLinkTraversal(); }
    }

    private void BeginLinkTraversal()
    {
        OffMeshLinkData link = _agent.currentOffMeshLinkData;
        _linkStart        = transform.position;
        _linkEnd          = link.endPos;
        _linkLength       = Vector3.Distance(_linkStart, _linkEnd);
        _linkProgress     = 0f;
        _rotationProgress = 0f;
        _traversingLink   = true;
        _agent.isStopped  = true;

        SetAgentRotation(false);

        _linkStartRot = transform.rotation;

        // Поворот на 90 по локальной X: пол стена нос вверх -90, стена пол нос вниз +90.
        float sign = (_linkEnd.y >= _linkStart.y) ? -1f : 1f;
        _linkEndRot = _linkStartRot * Quaternion.Euler(sign * 90f, 0f, 0f);
    }

    private void TickLinkTraversal()
    {
        if (_linkLength < 0.001f) { FinishLinkTraversal(); return; }

        float baseStep = _agent.speed * Time.deltaTime / _linkLength;

        _linkProgress     = Mathf.MoveTowards(_linkProgress,     1f, baseStep * _linkSpeedMultiplier);
        _rotationProgress = Mathf.MoveTowards(_rotationProgress, 1f, baseStep * _rotationSpeedMultiplier);

        transform.SetPositionAndRotation(
            Vector3.Lerp(_linkStart, _linkEnd, _linkProgress),
            Quaternion.Slerp(_linkStartRot, _linkEndRot, _rotationProgress));

        if (_linkProgress >= 1f)
            FinishLinkTraversal();
    }

    private void FinishLinkTraversal()
    {
        _traversingLink  = false;
        _agent.isStopped = false;
        _agent.CompleteOffMeshLink();
        SetAgentRotation(_isActive);
    }

    private void SetAgentRotation(bool enabled)
    {
        _agent.updateRotation = enabled;
        _agent.updateUpAxis   = enabled;
    }
}
