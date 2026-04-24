using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// <summary>
/// Версия MoveToAttackPositionAction без проверки прямой видимости игрока.
/// Используется для босса-гранатомётчика: он выходит из-за укрытия к ближайшей точке
/// в сторону последней известной позиции игрока и бросает гранату оттуда.
/// Не трогать оригинальный MoveToAttackPositionAction — он используется обычными врагами.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToAttackPositionNoLOS",
    story: "[Agent] moves to attack position (no LOS required)",
    category: "Action/Cover", id: "b7c3e1f9a4d82e0c6b5f3a17d2e94c01")]
public partial class MoveToAttackPositionNoLOSAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(4f);

    private EnemyBase        _enemy;
    private EnemyCoverModule _cover;
    private bool             _moving;
    private bool             _didPeek;
    private NavMeshPath      _reusablePath;

    protected override Status OnStart()
    {
        if (_enemy == null)
        {
            _enemy        = Agent.Value.GetComponent<EnemyBase>();
            _cover        = Agent.Value.GetComponent<EnemyCoverModule>();
            _reusablePath = new NavMeshPath();
        }

        if (_enemy == null || _cover == null) return Status.Failure;
        if (_enemy.State.IsDead)              return Status.Failure;

        _moving  = false;
        _didPeek = false;

        if (_enemy.State.PlayerIsSeen) return Status.Success;

        Vector3 coverOrigin = _enemy.State.HasCover
            ? _enemy.State.CurrentCoverPoint
            : _enemy.transform.position;

        if (!TryFindAttackPosition(coverOrigin, out Vector3 attackPos))
        {
            _cover.IncrementCoverIterations();
            return Status.Failure;
        }

        _enemy.Navigation.MoveTo(attackPos, run: true);
        _moving = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.IsDead) return Status.Failure;

        if (_moving)
        {
            if (_enemy.State.PlayerIsSeen) { _didPeek = true; return Status.Success; }
            if (_enemy.IsEnemyStopped())   { _moving = false; _didPeek = true; return Status.Success; }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_didPeek && _cover != null) _cover.IncrementCoverIterations();
        _moving  = false;
        _didPeek = false;
    }

    private bool TryFindAttackPosition(Vector3 coverOrigin, out Vector3 result)
    {
        result = Vector3.zero;
        if (_enemy.Navigation?.Agent == null) return false;

        // Направление в сторону последней известной позиции игрока
        Vector3 targetPos = _enemy.State.LastKnownPlayerPosition != Vector3.zero
            ? _enemy.State.LastKnownPlayerPosition
            : (_enemy.PlayerTransform != null ? _enemy.PlayerTransform.position : coverOrigin + _enemy.transform.forward);

        Vector3 toTarget = (targetPos - coverOrigin);
        toTarget.y = 0;
        if (toTarget == Vector3.zero) toTarget = _enemy.transform.forward;
        toTarget.Normalize();

        float radius      = SearchRadius.Value;
        float bestPathLen = Mathf.Infinity;
        bool  found       = false;

        for (int i = 0; i < 16; i++)
        {
            Vector3 rand = UnityEngine.Random.insideUnitSphere;
            rand.y = 0;
            rand.Normalize();

            // Смещаем кандидата в сторону игрока, чтобы босс выходил из-за укрытия в правильном направлении
            Vector3 dir       = Vector3.Lerp(rand, toTarget, 0.6f).normalized;
            Vector3 candidate = coverOrigin + dir * radius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                continue;

            if (!_enemy.Navigation.Agent.CalculatePath(hit.position, _reusablePath) ||
                _reusablePath.status != NavMeshPathStatus.PathComplete)
                continue;

            float pathLen = GetPathLength(_reusablePath);
            if (pathLen < bestPathLen)
            {
                bestPathLen = pathLen;
                result      = hit.position;
                found       = true;
            }
        }

        return found;
    }

    private static float GetPathLength(NavMeshPath path)
    {
        float     len     = 0f;
        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
            len += Vector3.Distance(corners[i - 1], corners[i]);
        return len;
    }
}

/// <summary>
/// Версия AlwaysThrowGrenadeAtPlayerAction без проверки PlayerDetected.
/// Нужна для босса: пока он прячется за укрытием и не видит игрока,
/// система восприятия может сбросить PlayerDetected. Этот нод игнорирует это
/// и кидает гранату пока есть PlayerTransform и нет кулдауна.
/// Не трогать оригинальный AlwaysThrowGrenadeAtPlayerAction.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BossThrowGrenade",
    story: "[Agent] always throws grenade at player (boss)",
    category: "Action", id: "c9f4a2b1e7d53c8f0a6b4e2d1f97c3e5")]
public partial class BossThrowGrenadeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase          _enemy;
    private GrenadeThrowModule _grenadeModule;
    private float              _maxDuration;
    private float              _elapsed;

    protected override Status OnStart()
    {
        _enemy ??= Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null || _enemy.PlayerTransform == null) return Status.Failure;
        if (_enemy.State.IsDead)                              return Status.Failure;

        _grenadeModule = _enemy.GetComponent<GrenadeThrowModule>();

        if (_grenadeModule == null || !_grenadeModule.CanThrowGrenade) return Status.Failure;

        _enemy.Navigation.Stop();
        _grenadeModule.StartWindUp();

        _maxDuration = _grenadeModule.TotalDuration + 1f;
        _elapsed     = 0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.IsDead) return Status.Failure;

        _elapsed += Time.deltaTime;
        if (_elapsed >= _maxDuration)
        {
            _grenadeModule.Cancel();
            return Status.Failure;
        }

        RotateTowardsPlayer();

        if (_grenadeModule.Phase == GrenadeThrowPhase.Idle)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _grenadeModule?.Cancel();
        _enemy?.Navigation.Resume();
    }

    private void RotateTowardsPlayer()
    {
        if (_enemy.PlayerTransform == null) return;
        Vector3 dir = (_enemy.PlayerTransform.position - _enemy.transform.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) return;
        _enemy.transform.rotation = Quaternion.Slerp(
            _enemy.transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * _enemy.Navigation.RotationSpeed
        );
    }
}
