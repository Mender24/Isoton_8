using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChasePlayer", story: "[Agent] chases player",
    category: "Action", id: "a1d208cde47eb49f2e8a401e2af81d95")]
public partial class ChasePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e == null || e.PlayerTransform == null) return Status.Failure;
        e.Navigation.SetSpeed(e.Navigation.RunSpeed);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;
        if (!e.State.PlayerDetected || e.PlayerTransform == null) return Status.Failure;

        e.Navigation.MoveTo(e.PlayerTransform.position);

        if (e is MeleeEnemy melee && melee.IsInMeleeRange())
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _enemy?.Navigation.ResetPath();
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttackPlayer", story: "[Agent] attacks in melee",
    category: "Action", id: "98f53f4ecb43511e60125c3eca86d27c")]
public partial class MeleeAttackPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    private MeleeEnemy _melee;
    private bool _hasInitiatedAttack;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null || _enemy.PlayerTransform == null) return Status.Failure;

        _melee = _enemy as MeleeEnemy;
        if (_melee == null)
        {
            Debug.LogWarning("[MeleeAttackPlayerAction] Enemy is not MeleeEnemy!");
            return Status.Failure;
        }

        _hasInitiatedAttack = false;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;
        if (e.PlayerTransform == null || !e.State.PlayerDetected) return Status.Failure;

        RotateTowardsPlayer(e);

        float angle = Quaternion.Angle(e.transform.rotation,
            Quaternion.LookRotation(GetFlatDir(e)));

        if (angle < 5f && !_hasInitiatedAttack &&
            _melee.MeleeCombat.CanAttack && _melee.IsInMeleeRange())
        {
            _hasInitiatedAttack = true;

            bool inMotion = e.Navigation.CurrentSpeed > 0.1f;
            if (!inMotion) e.Navigation.Stop();

            e.StartAttack();
        }

        if (e.State.IsMeleeAttacking) return Status.Running;

        if (!_melee.IsInMeleeRange())
        {
            e.Navigation.Resume();
            return Status.Failure;
        }

        if (_hasInitiatedAttack) // end attack
        {
            e.Navigation.Resume();
            return Status.Success;
        }

        return Status.Running;
    }

    private void RotateTowardsPlayer(EnemyBase e)
    {
        Vector3 dir = GetFlatDir(e);
        if (dir == Vector3.zero) return;
        e.transform.rotation = Quaternion.Slerp(
            e.transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * e.Navigation.RotationSpeed
        );
    }

    private Vector3 GetFlatDir(EnemyBase e)
    {
        Vector3 dir = (e.PlayerTransform.position - e.transform.position).normalized;
        dir.y = 0;
        return dir;
    }
}
