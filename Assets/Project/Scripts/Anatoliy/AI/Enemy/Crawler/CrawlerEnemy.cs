using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ползающий враг ближнего боя.
/// Наследует EnemyBase, использует стандартный NavMeshAgent + EnemyNavigation.
/// NavMesh запекается на каждой поверхности комнаты x6 + NavMeshLink на рёбрах.
/// Ориентацию по нормали поверхности обеспечивает CrawlerSurfaceAligner.
/// </summary>
[RequireComponent(typeof(MeleeCombatModule))]
[RequireComponent(typeof(CrawlerSurfaceAligner))]
public class CrawlerEnemy : EnemyBase
{
    [Header("Player Visibility")]
    [SerializeField] private float     _playerFOV       = 120f;
    [SerializeField] private float     _playerEyeHeight = 1.8f;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("NavMesh Helpers")]
    [SerializeField] private float _safeSpotRadius    = 12f;
    [SerializeField] private float _ambushRadius      = 9f;
    [SerializeField] private float _leapLandRadius    = 1.4f;
    [SerializeField] private int   _navSearchAttempts = 20;

    [Header("Ceiling/Wall Attack")]
    [SerializeField] private float _ceilingMinHeight  = 2.0f;
    [SerializeField] private float _ceilingAttackDist = 8f;
    [SerializeField] private float _dropZoneMinDist   = 1.5f;
    [SerializeField] private float _dropZoneMaxDist   = 3.0f;

    [Header("Shot Memory")]
    [SerializeField] private float _shotMemoryTime = 2f;

    public bool              WasRecentlyShot => _shotTimer > 0f;
    public MeleeCombatModule MeleeCombat     => _meleeCombat;

    public bool SurfaceAlignmentActive
    {
        get => _aligner != null && _aligner.IsActive;
        set { if (_aligner != null) _aligner.IsActive = value; }
    }

    private MeleeCombatModule    _meleeCombat;
    private CrawlerAnimator      _crawlerAnimator;
    private CrawlerSurfaceAligner _aligner;
    private float                _shotTimer;

    protected override void Awake()
    {
        base.Awake();
        _meleeCombat     = GetComponent<MeleeCombatModule>();
        _crawlerAnimator = GetComponent<CrawlerAnimator>();
        _aligner         = GetComponent<CrawlerSurfaceAligner>();
    }

    protected override void OnInitialized()
    {
        _meleeCombat.Initialize(PlayerTransform);

        if (_crawlerAnimator != null)
            _crawlerAnimator.OnMeleeHit += _meleeCombat.ExecuteHit;
    }

    private void OnDestroy()
    {
        if (_crawlerAnimator != null)
            _crawlerAnimator.OnMeleeHit -= _meleeCombat.ExecuteHit;
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;

        if (_shotTimer > 0f)
            _shotTimer -= Time.deltaTime;

        _meleeCombat.Tick(Time.deltaTime);
    }

    protected override void OnDamaged(float amount, GameObject source)
    {
        base.OnDamaged(amount, source);
        NotifyShot();
    }

    public override bool CanAttack()   => _meleeCombat.CanAttack;
    public override void StartAttack() => _meleeCombat.StartAttack();

    public void NotifyShot() => _shotTimer = _shotMemoryTime;

    public bool IsVisibleToPlayer()
    {
        if (PlayerTransform == null) return false;

        Vector3 playerEye = PlayerTransform.position + Vector3.up * _playerEyeHeight;
        Vector3 toSelf    = transform.position - playerEye;
        float   dist      = toSelf.magnitude;

        Camera cam = Camera.main;
        if (cam != null)
        {
            if (Vector3.Angle(cam.transform.forward, toSelf.normalized) > _playerFOV * 0.5f)
                return false;
        }

        return !Physics.Raycast(playerEye, toSelf.normalized, dist - 0.05f, _obstacleLayer);
    }

    public bool IsPositionVisibleToPlayer(Vector3 pos)
    {
        if (PlayerTransform == null) return false;
        Vector3 playerEye = PlayerTransform.position + Vector3.up * _playerEyeHeight;
        Vector3 toPos     = pos - playerEye;
        return !Physics.Raycast(playerEye, toPos.normalized, toPos.magnitude - 0.05f, _obstacleLayer);
    }

    public bool TryFindSafeNavPoint(out Vector3 result) =>
        TryFindSafeNavPoint(transform.position, _safeSpotRadius, out result);

    public bool TryFindSafeNavPoint(Vector3 origin, float radius, out Vector3 result)
    {
        result = Vector3.zero;
        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * radius + origin;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius, NavMesh.AllAreas)
                && !IsPositionVisibleToPlayer(hit.position))
            {
                result = hit.position;
                return true;
            }
        }
        return false;
    }

    public bool TryFindAmbushPosition(out Vector3 result)
    {
        result = Vector3.zero;
        if (PlayerTransform == null) return false;

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * _ambushRadius + PlayerTransform.position;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _ambushRadius, NavMesh.AllAreas)
                && !IsPositionVisibleToPlayer(hit.position))
            {
                result = hit.position;
                return true;
            }
        }
        return false;
    }

    public bool TryFindCeilingWallNearPlayer(out Vector3 result)
    {
        result = Vector3.zero;
        if (PlayerTransform == null) return false;

        float playerY = PlayerTransform.position.y;

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * _ambushRadius + PlayerTransform.position;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _ambushRadius, NavMesh.AllAreas)
                && hit.position.y > playerY + _ceilingMinHeight
                && !IsPositionVisibleToPlayer(hit.position))
            {
                result = hit.position;
                return true;
            }
        }

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * _ambushRadius + PlayerTransform.position;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _ambushRadius, NavMesh.AllAreas)
                && hit.position.y > playerY + _ceilingMinHeight)
            {
                result = hit.position;
                return true;
            }
        }

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * _safeSpotRadius + transform.position;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _safeSpotRadius, NavMesh.AllAreas)
                && hit.position.y > playerY + _ceilingMinHeight)
            {
                result = hit.position;
                return true;
            }
        }

        return false;
    }

    public bool IsPositionedOnCeilingWallForAttack()
    {
        if (PlayerTransform == null) return false;
        float playerY = PlayerTransform.position.y;
        if (transform.position.y <= playerY + _ceilingMinHeight) return false;

        Vector3 toPlayer = PlayerTransform.position - transform.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude <= _ceilingAttackDist;
    }

    public bool TryFindDropZoneBeforePlayer(out Vector3 result)
    {
        result = transform.position;
        if (PlayerTransform == null) return false;

        Vector3 facing = PlayerTransform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f) facing = Vector3.forward;
        facing.Normalize();

        float playerY = PlayerTransform.position.y;

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            float   angle = Random.Range(-50f, 50f);
            Vector3 dir   = Quaternion.Euler(0f, angle, 0f) * facing;
            float   dist  = Random.Range(_dropZoneMinDist, _dropZoneMaxDist);
            Vector3 cand  = PlayerTransform.position + dir * dist;

            if (NavMesh.SamplePosition(cand, out NavMeshHit hit, 2f, NavMesh.AllAreas)
                && Mathf.Abs(hit.position.y - playerY) < 0.5f)
            {
                result = hit.position;
                return true;
            }
        }
        return false;
    }

    public bool TryFindWallCeilingSafeSpot(out Vector3 result)
    {
        result = Vector3.zero;
        float playerY = PlayerTransform != null ? PlayerTransform.position.y : 0f;

        for (int i = 0; i < _navSearchAttempts; i++)
        {
            Vector3 candidate = Random.insideUnitSphere * _safeSpotRadius + transform.position;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _safeSpotRadius, NavMesh.AllAreas)
                && hit.position.y > playerY + _ceilingMinHeight
                && !IsPositionVisibleToPlayer(hit.position))
            {
                result = hit.position;
                return true;
            }
        }
        return TryFindSafeNavPoint(out result);
    }

    public Vector3 FindLandingPoint()
    {
        if (PlayerTransform == null) return transform.position;

        Vector2 rand2D = Random.insideUnitCircle.normalized * _leapLandRadius;
        Vector3 target = PlayerTransform.position + new Vector3(rand2D.x, 0f, rand2D.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return target;
    }
}
