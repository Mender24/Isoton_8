using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Скриптованный враг, ползающий по потолку.
/// Логика:
///   CeilingPatrol  — ходит по точкам патруля на потолке
///   Dropping       — прыгает по дуге вниз рядом с игроком (триггер извне)
///   Attacking      — преследует и атакует игрока ближнего боя
///   Fleeing        — убегает от игрока в случайное безопасное место
///   Returning      — ждёт, затем возвращается на потолок и возобновляет патруль
///
/// Не использует BehaviorGraphAgent — поведение полностью скриптованное.
/// Активируется самостоятельно (Start). Сброс с потолка вызывается через TriggerDrop().
/// </summary>
[RequireComponent(typeof(MeleeCombatModule))]
[RequireComponent(typeof(CrawlerSurfaceAligner))]
public class ScriptedCeilingCrawler : EnemyBase
{
    private enum Phase { CeilingPatrol, Dropping, Attacking, Fleeing, Returning, SeekingDropPoint }

    [Header("Attack")]
    [SerializeField] private float _attackDuration  = 5f;

    [Header("Flee")]
    [SerializeField] private float _fleeRadius      = 12f;
    [SerializeField] private int   _fleeAttempts    = 20;

    [Header("Return")]
    [SerializeField] private float _returnDelay     = 6f;

    [Header("Drop Arc")]
    [SerializeField] private float     _dropDuration    = 0.45f;
    [SerializeField] private float     _dropArcHeight   = 1.5f;
    [SerializeField] private float     _dropZoneMinDist = 1.5f;
    [SerializeField] private float     _dropZoneMaxDist = 3.0f;
    [SerializeField] private int       _dropAttempts    = 20;

    [Header("Obstacle Check")]
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Seek Drop Point")]
    [SerializeField] private float _seekTimeout       = 8f;
    [SerializeField] private float _seekCheckInterval = 0.5f;

    private Phase                _phase = Phase.CeilingPatrol;
    private MeleeCombatModule    _meleeCombat;
    private CrawlerAnimator      _crawlerAnimator;
    private CrawlerSurfaceAligner _aligner;

    private int  _patrolIndex;
    private bool _patrolWaiting;

    private Vector3 _dropStart;
    private Vector3 _dropEnd;
    private float   _dropElapsed;

    private float _attackTimer;

    private float _returnTimer;
    private bool  _isNavigatingBack;

    private float _seekTimer;
    private float _seekCheckTimer;

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

        Activate();
        EnterPhase(Phase.CeilingPatrol);
    }

    private void OnDestroy()
    {
        if (_crawlerAnimator != null)
            _crawlerAnimator.OnMeleeHit -= _meleeCombat.ExecuteHit;
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;

        if (_phase == Phase.Attacking)
            _meleeCombat.Tick(Time.deltaTime);

        switch (_phase)
        {
            case Phase.CeilingPatrol:    TickPatrol();            break;
            case Phase.Dropping:         TickDropping();          break;
            case Phase.Attacking:        TickAttacking();         break;
            case Phase.Fleeing:          TickFleeing();           break;
            case Phase.Returning:        TickReturning();         break;
            case Phase.SeekingDropPoint: TickSeekingDropPoint();  break;
        }
    }

    public void TriggerDrop()
    {
        if (!State.IsActivated || State.IsDead) return;
        if (_phase != Phase.CeilingPatrol)       return;

        EnterPhase(Phase.Dropping);
    }

    public override bool CanAttack()   => _meleeCombat.CanAttack;
    public override void StartAttack() => _meleeCombat.StartAttack();

    private void EnterPhase(Phase next)
    {
        _phase = next;

        switch (next)
        {
            case Phase.CeilingPatrol:
                _aligner.IsActive               = true;
                Navigation.Agent.updatePosition = true;
                State.PlayerDetected            = false;
                State.IsAlerted                 = false;
                State.IsMeleeAttacking          = false;
                State.MeleeAttackCooldown       = 0f;
                Animator?.SetMeleeAttacking(false, 0f, false);
                Navigation.Unlock();
                StartCeilingPatrol();
                break;

            case Phase.Dropping:
                BeginDrop();
                break;

            case Phase.Attacking:
                _aligner.IsActive    = true;
                State.IsAlerted      = true;
                State.PlayerDetected = true;
                _attackTimer         = _attackDuration;
                State.MeleeAttackCooldown = 0f;
                Navigation.Unlock();
                Navigation.SetSpeed(Navigation.RunSpeed);
                break;

            case Phase.Fleeing:
                State.IsMeleeAttacking    = false;
                State.MeleeAttackCooldown = 0f;
                Animator?.SetMeleeAttacking(false, 0f, false);
                State.PlayerDetected   = false;
                BeginFlee();
                break;

            case Phase.Returning:
                _returnTimer      = _returnDelay;
                _isNavigatingBack = false;
                Navigation.Stop();
                break;

            case Phase.SeekingDropPoint:
                _seekTimer      = 0f;
                _seekCheckTimer = _seekCheckInterval; // check immediately on first tick
                BeginSeekDropPoint();
                break;
        }
    }

    private void StartCeilingPatrol()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0) return;
        MoveToCurrentPatrolPoint();
    }

    private void MoveToCurrentPatrolPoint()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0) return;
        var pt = PatrolPoints[_patrolIndex];
        if (pt != null)
            Navigation.MoveTo(pt.transform.position, run: false);
    }

    private void TickPatrol()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0) return;
        if (_patrolWaiting) return;

        if (Navigation.HasReachedDestination())
        {
            _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Count;

            if (WaypointWaitTime > 0f)
                StartCoroutine(PatrolWaitRoutine());
            else
                MoveToCurrentPatrolPoint();
        }
    }

    private IEnumerator PatrolWaitRoutine()
    {
        _patrolWaiting = true;
        yield return new WaitForSeconds(WaypointWaitTime);
        _patrolWaiting = false;

        if (_phase == Phase.CeilingPatrol)
            MoveToCurrentPatrolPoint();
    }

    private void BeginDrop()
    {
        Navigation.Agent.isStopped      = true;
        Navigation.Agent.updatePosition = false;
        _aligner.IsActive               = false;

        if (TryFindDropZone(out _dropEnd))
        {
            _dropStart   = transform.position;
            _dropElapsed = 0f;
            return;
        }

        // Fallback: directly below, still with LoS check
        Vector3 below = transform.position + Vector3.down * 6f;
        if (NavMesh.SamplePosition(below, out NavMeshHit hit, 6f, NavMesh.AllAreas)
            && !Physics.Linecast(transform.position, hit.position, _obstacleLayer))
        {
            _dropEnd     = hit.position;
            _dropStart   = transform.position;
            _dropElapsed = 0f;
            return;
        }

        // No clear drop path from here — navigate on ceiling toward player
        Navigation.Agent.updatePosition = true;
        Navigation.Agent.isStopped      = false;
        _aligner.IsActive               = true;
        EnterPhase(Phase.SeekingDropPoint);
    }

    private void TickDropping()
    {
        _dropElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_dropElapsed / _dropDuration);

        Vector3 pos = Vector3.Lerp(_dropStart, _dropEnd, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * _dropArcHeight;
        transform.position = pos;

        Vector3 facingDir;
        if (t > 0.5f && PlayerTransform != null)
        {
            facingDir = PlayerTransform.position - transform.position;
            facingDir.y = 0f;
        }
        else
        {
            facingDir = _dropEnd - _dropStart;
            facingDir.y = 0f;
        }

        if (facingDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(facingDir.normalized, Vector3.up),
                15f * Time.deltaTime);
        }

        if (t < 1f) return;

        transform.position = _dropEnd;
        Navigation.Agent.Warp(_dropEnd);
        Navigation.Agent.updatePosition = true;
        Navigation.Agent.isStopped      = false;
        _aligner.IsActive               = true;

        EnterPhase(Phase.Attacking);
    }

    private bool TryFindDropZone(out Vector3 result)
    {
        result = transform.position;
        if (PlayerTransform == null) return false;

        Vector3 facing = PlayerTransform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f) facing = Vector3.forward;
        facing.Normalize();

        float playerY = PlayerTransform.position.y;

        for (int i = 0; i < _dropAttempts; i++)
        {
            float   angle = Random.Range(-50f, 50f);
            Vector3 d     = Quaternion.Euler(0f, angle, 0f) * facing;
            float   dist  = Random.Range(_dropZoneMinDist, _dropZoneMaxDist);
            Vector3 cand  = PlayerTransform.position + d * dist;

            if (NavMesh.SamplePosition(cand, out NavMeshHit hit, 2f, NavMesh.AllAreas)
                && Mathf.Abs(hit.position.y - playerY) < 0.5f
                && !Physics.Linecast(transform.position, hit.position, _obstacleLayer))
            {
                result = hit.position;
                return true;
            }
        }
        return false;
    }

    private void TickAttacking()
    {
        if (PlayerTransform == null) { EnterPhase(Phase.Fleeing); return; }

        _attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, PlayerTransform.position);
        if (dist > _meleeCombat.AttackRange)
        {
            Navigation.MoveTo(PlayerTransform.position, run: true);
        }
        else
        {
            Navigation.Stop();
            if (_meleeCombat.CanAttack)
                _meleeCombat.StartAttack();
        }

        if (_attackTimer <= 0f)
            EnterPhase(Phase.Fleeing);
    }

    private void BeginFlee()
    {
        if (!TryFindFleePoint(out Vector3 target))
        {
            if (PlayerTransform != null)
            {
                Vector3 away = (transform.position - PlayerTransform.position).normalized;
                Vector3 cand = transform.position + away * _fleeRadius;
                if (NavMesh.SamplePosition(cand, out NavMeshHit h, _fleeRadius, NavMesh.AllAreas))
                    target = h.position;
                else
                    target = transform.position;
            }
            else target = transform.position;
        }

        Navigation.MoveTo(target, run: true);
    }

    private bool TryFindFleePoint(out Vector3 result)
    {
        result = Vector3.zero;
        for (int i = 0; i < _fleeAttempts; i++)
        {
            Vector3 cand = Random.insideUnitSphere * _fleeRadius + transform.position;
            if (NavMesh.SamplePosition(cand, out NavMeshHit hit, _fleeRadius, NavMesh.AllAreas))
            {
                if (PlayerTransform == null ||
                    Vector3.Distance(hit.position, PlayerTransform.position) > _fleeRadius * 0.4f)
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        return false;
    }

    private void TickFleeing()
    {
        if (Navigation.HasReachedDestination())
            EnterPhase(Phase.Returning);
    }

    private void TickReturning()
    {
        if (!_isNavigatingBack)
        {
            _returnTimer -= Time.deltaTime;
            if (_returnTimer <= 0f)
            {
                _isNavigatingBack = true;
                NavigateBackToCeiling();
            }
        }
        else
        {
            if (Navigation.HasReachedDestination())
                EnterPhase(Phase.CeilingPatrol);
        }
    }

    private void BeginSeekDropPoint()
    {
        if (PlayerTransform == null) { EnterPhase(Phase.CeilingPatrol); return; }

        // Move along the ceiling surface toward the position directly above the player
        Vector3 abovePlayer = new Vector3(
            PlayerTransform.position.x,
            transform.position.y,
            PlayerTransform.position.z);

        if (NavMesh.SamplePosition(abovePlayer, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            Navigation.MoveTo(hit.position, run: false);
    }

    private void TickSeekingDropPoint()
    {
        if (PlayerTransform == null) { EnterPhase(Phase.CeilingPatrol); return; }

        _seekTimer      += Time.deltaTime;
        _seekCheckTimer += Time.deltaTime;

        if (_seekTimer >= _seekTimeout)
        {
            EnterPhase(Phase.CeilingPatrol);
            return;
        }

        if (_seekCheckTimer < _seekCheckInterval) return;
        _seekCheckTimer = 0f;

        if (TryFindDropZone(out _dropEnd))
        {
            Navigation.Agent.isStopped      = true;
            Navigation.Agent.updatePosition = false;
            _aligner.IsActive               = false;
            _dropStart   = transform.position;
            _dropElapsed = 0f;
            _phase       = Phase.Dropping;
            return;
        }

        // Player may have moved — refresh navigation target on ceiling
        Vector3 abovePlayer = new Vector3(
            PlayerTransform.position.x,
            transform.position.y,
            PlayerTransform.position.z);
        if (NavMesh.SamplePosition(abovePlayer, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            Navigation.MoveTo(navHit.position, run: false);
    }

    private void NavigateBackToCeiling()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0)
        {
            EnterPhase(Phase.CeilingPatrol);
            return;
        }

        var pt = PatrolPoints[_patrolIndex];
        if (pt != null)
            Navigation.MoveTo(pt.transform.position, run: true);
        else
            EnterPhase(Phase.CeilingPatrol);
    }

    public override void FullReset()
    {
        StopAllCoroutines();

        _phase            = Phase.CeilingPatrol;
        _patrolIndex      = 0;
        _patrolWaiting    = false;
        _isNavigatingBack = false;
        _returnTimer      = 0f;
        _dropElapsed      = 0f;
        _attackTimer      = 0f;
        _seekTimer        = 0f;
        _seekCheckTimer   = 0f;

        Navigation.Agent.updatePosition = true;

        base.FullReset();

        if (_aligner != null) _aligner.IsActive = true;

        Activate();
        EnterPhase(Phase.CeilingPatrol);
    }

    protected override void OnDamaged(float amount, GameObject source)
    {
        base.OnDamaged(amount, source);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        if (_aligner != null)
            _aligner.IsActive = false;
    }
}
