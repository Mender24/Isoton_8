using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Компонент системы укрытий. Добавляется только на врагов с укрытиями.
/// </summary>
public class EnemyCoverModule : MonoBehaviour
{
    [Header("Search")]
    [SerializeField] private LayerMask _coverLayers;
    [SerializeField] private float _searchRadius = 20f;
    [SerializeField] private float _minCoverHeight = 0.8f;
    [Tooltip("Чувствительность укрытия. -1=только идеальные, 0=хорошие, 0.5=любые.")]
    [SerializeField, Range(-1f, 1f)] private float _hideSensitivity = -0.25f;
    [Tooltip("Минимальная дистанция от укрытия до позиции игрока.")]
    [SerializeField] private float _minDistanceFromPlayer = 3f;

    [Header("Behaviour")]
    [Tooltip("0=всегда в укрытие, 10=никогда. Вероятность выбрать укрытие против прямой атаки.")]
    [SerializeField, Range(0, 10)] private int _bravery = 3;
    [SerializeField] private float _attackFromCoverCooldown = 2f;

    [Header("Peek (выход для атаки)")]
    [SerializeField] private float _peekSearchRadius = 4f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private bool _showDebug = false;

    public event Action OnCoverOccupied;
    public event Action OnCoverReleased;
    public event Action OnCoverBlown;

    public bool HasCover       => _state != null && _state.HasCover;
    public bool IsInCover      => _state != null && _state.IsInCover;
    public float AttackCooldown => _attackFromCoverCooldown;

    private EnemyState      _state;
    private EnemyNavigation _navigation;
    private Transform       _playerTransform;
    private CoverPoint      _currentCoverPoint;
    private bool?           _cachedCoverDecision;

    private readonly Collider[] _searchBuffer = new Collider[32];

    private void Awake()
    {
        _state      = GetComponent<EnemyState>();
        _navigation = GetComponent<EnemyNavigation>();
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public bool FindAndOccupyCover()
    {
        if (_state.HasCover && !IsCoverBlown()) return true;

        ReleaseCover();

        List<CoverCandidate> candidates = CollectCandidates();
        if (candidates.Count == 0) return false;

        candidates.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        OccupyCover(candidates[0]);
        return true;
    }

    public void ReleaseCover()
    {
        if (_currentCoverPoint != null)
            _currentCoverPoint.Release();

        _currentCoverPoint = null;

        if (_state != null)
        {
            _state.HasCover            = false;
            _state.IsInCover           = false;
            _state.CurrentCoverPoint   = Vector3.zero;
            _state.CurrentCoverObject  = null;
        }

        _cachedCoverDecision = null;
        OnCoverReleased?.Invoke();
    }

    /// <summary>
    /// Проверяет, видит ли игрок AI из текущей позиции.
    /// Если да укрытие не подходит.
    /// </summary>
    public bool IsCoverBlown()
    {
        if (_playerTransform == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 dir    = (_playerTransform.position + Vector3.up - eyePos).normalized;
        float   dist   = Vector3.Distance(eyePos, _playerTransform.position);

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist + 1f, _obstacleMask | LayerMaskFromPlayer()))
        {
            bool blown = hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform);
            if (blown) OnCoverBlown?.Invoke();
            return blown;
        }

        OnCoverBlown?.Invoke();
        return true;
    }

    /// <summary>Стоит ли AI достаточно близко к своей cover point.</summary>
    public bool IsAtCoverPoint()
    {
        if (_state == null || !_state.HasCover) return false;
        return Vector3.Distance(transform.position, _state.CurrentCoverPoint) < 0.8f;
    }

    /// <summary>
    /// Ищет навигационную точку поблизости, из которой виден игрок.
    /// Используется для peek перед атакой.
    /// </summary>
    public bool TryGetPeekPosition(out Vector3 peekPos)
    {
        peekPos = Vector3.zero;
        if (_playerTransform == null) return false;

        Vector3 coverOrigin = _state != null && _state.HasCover
            ? _state.CurrentCoverPoint
            : transform.position;

        for (int i = 0; i < 12; i++)
        {
            Vector3 candidateDir = UnityEngine.Random.insideUnitSphere;
            candidateDir.y = 0;
            Vector3 candidate = coverOrigin + candidateDir.normalized * _peekSearchRadius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas))
                continue;

            if (!HasLineOfSightToPlayer(navHit.position))
                continue;

            if (!_navigation.Agent.CalculatePath(navHit.position, new NavMeshPath()))
                continue;

            peekPos = navHit.position;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Решение нужно ли идти в укрытие.
    /// Bravery=0 всегда. Bravery=10 никогда.
    /// </summary>
    public bool ShouldTakeCover()
    {
        if (_cachedCoverDecision.HasValue)
            return _cachedCoverDecision.Value;

        bool decision;
        if (_bravery == 0)       decision = true;
        else if (_bravery == 10) decision = false;
        else
        {
            int odds = 10 - _bravery;
            decision = UnityEngine.Random.Range(1, 11) <= odds;
        }

        _cachedCoverDecision = decision;
        return decision;
    }

    public void ResetCoverDecision() => _cachedCoverDecision = null;

    /// <summary>Commander назначает конкретное укрытие.</summary>
    public void ForceSetCover(Vector3 coverPoint, Transform coverObject)
    {
        ReleaseCover();

        if (_state != null)
        {
            _state.HasCover           = true;
            _state.CurrentCoverPoint  = coverPoint;
            _state.CurrentCoverObject = coverObject;
        }

        OnCoverOccupied?.Invoke();
    }

    /// <summary>Commander выгоняет из укрытия в атаку.</summary>
    public void ForceReleaseCover() => ReleaseCover();

    private List<CoverCandidate> CollectCandidates()
    {
        var candidates = new List<CoverCandidate>();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, _searchRadius, _searchBuffer, _coverLayers);

        for (int i = 0; i < count; i++)
        {
            Collider col = _searchBuffer[i];
            if (col == null) continue;

            CoverPoint cp = col.GetComponent<CoverPoint>();
            if (cp != null && cp.IsOccupied && cp.OccupiedBy != transform) continue;

            float height = cp != null ? cp.GetHeight() : col.bounds.size.y;
            if (height < _minCoverHeight) continue;

            if (_playerTransform != null &&
                Vector3.Distance(col.transform.position, _playerTransform.position) < _minDistanceFromPlayer)
                continue;

            if (cp != null && cp.CustomPositions != null && cp.CustomPositions.Length > 0)
            {
                if (TryGetCustomPosition(cp, col, out CoverCandidate customCand))
                    candidates.Add(customCand);
                continue;
            }

            if (TryGetNavMeshEdgePosition(col, out CoverCandidate navCand))
                candidates.Add(navCand);
        }

        return candidates;
    }

    private bool TryGetCustomPosition(CoverPoint cp, Collider col, out CoverCandidate candidate)
    {
        candidate = default;
        float bestDist = Mathf.Infinity;
        Vector3 bestPos = Vector3.zero;
        bool found = false;

        foreach (var localPos in cp.CustomPositions)
        {
            Vector3 worldPos = col.transform.TransformPoint(localPos);

            if (!NavMesh.SamplePosition(worldPos, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas))
                continue;

            if (HasLineOfSightToPlayer(navHit.position))
                continue;

            if (!_navigation.Agent.CalculatePath(navHit.position, new NavMeshPath()))
                continue;

            float dist = Vector3.Distance(transform.position, navHit.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPos  = navHit.position;
                found    = true;
            }
        }

        if (!found) return false;

        candidate = new CoverCandidate(bestPos, col.transform, cp, bestDist);
        return true;
    }

    private bool TryGetNavMeshEdgePosition(Collider col, out CoverCandidate candidate)
    {
        candidate = default;

        if (!NavMesh.FindClosestEdge(col.transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
            return false;

        Vector3 edgePos    = edgeHit.position;
        Vector3 edgeNormal = edgeHit.normal;

        if (_playerTransform != null)
        {
            Vector3 dirToPlayer = (_playerTransform.position - edgePos).normalized;
            float dot = Vector3.Dot(edgeNormal, dirToPlayer);
            if (dot > _hideSensitivity) return false;
        }

        if (!NavMesh.SamplePosition(edgePos, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
            return false;

        if (HasLineOfSightToPlayer(navHit.position))
            return false;

        if (!_navigation.Agent.CalculatePath(navHit.position, new NavMeshPath()))
            return false;

        float dist = Vector3.Distance(transform.position, navHit.position);
        candidate = new CoverCandidate(navHit.position, col.transform, null, dist);
        return true;
    }

    private void OccupyCover(CoverCandidate best)
    {
        _currentCoverPoint = best.CoverPoint;
        _currentCoverPoint?.Occupy(transform);

        if (_state != null)
        {
            _state.HasCover           = true;
            _state.IsInCover          = false;
            _state.CurrentCoverPoint  = best.Position;
            _state.CurrentCoverObject = best.CoverObject;
        }

        OnCoverOccupied?.Invoke();
    }

    private bool HasLineOfSightToPlayer(Vector3 fromPos)
    {
        if (_playerTransform == null) return false;

        Vector3 eyePos = fromPos + Vector3.up * 1.5f;
        Vector3 target = _playerTransform.position + Vector3.up;
        Vector3 dir    = (target - eyePos).normalized;
        float   dist   = Vector3.Distance(eyePos, target);

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist + 1f, _obstacleMask | LayerMaskFromPlayer()))
            return hit.transform == _playerTransform || hit.transform.IsChildOf(_playerTransform);

        return true;
    }

    private LayerMask LayerMaskFromPlayer()
    {
        if (_playerTransform == null) return 0;
        return 1 << _playerTransform.gameObject.layer;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_showDebug) return;
        
        UnityEditor.Handles.color = new Color(0f, 1f, 0.5f, 0.1f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _searchRadius);
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, _searchRadius);

        if (_state != null && _state.HasCover)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_state.CurrentCoverPoint, 0.4f);
            Gizmos.DrawLine(transform.position, _state.CurrentCoverPoint);
        }
    }
#endif

    private readonly struct CoverCandidate
    {
        public readonly Vector3    Position;
        public readonly Transform  CoverObject;
        public readonly CoverPoint CoverPoint;
        public readonly float      Distance;

        public CoverCandidate(Vector3 pos, Transform obj, CoverPoint cp, float dist)
        {
            Position    = pos;
            CoverObject = obj;
            CoverPoint  = cp;
            Distance    = dist;
        }
    }
}
