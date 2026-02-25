using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AimAndShootAtPlayer",
    story: "[Agent] aims at player with animation and starts shooting",
    category: "Action", id: "ff484dcfaa9e70c54238b5011ba6816a")]
public class AimAndShootAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase _enemy;
    private bool _hasStartedAiming;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e == null || e.PlayerTransform == null) return Status.Failure;

        _hasStartedAiming = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;
        if (e.PlayerTransform == null || !e.State.PlayerDetected) return Status.Failure;

        if (e.State.IsAlertAnimationPlaying) return Status.Running;

        if (!_hasStartedAiming)
        {
            e.Navigation.Stop();
            e.Animator?.SetAiming(true);
        }

        RotateTowardsPlayer(e);

        float angle = Quaternion.Angle(e.transform.rotation, Quaternion.LookRotation(GetFlatDir(e)));

        if (angle < 2.5f && !_hasStartedAiming)
        {
            _hasStartedAiming = true;
            e.StartAttack();
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        var e = _enemy;
        if (e == null) return;

        if (e is RangedEnemy ranged)
            ranged.RangedCombat.StopFire();

        e.Animator?.SetAiming(false);
        e.Navigation.Resume();
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

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AlwaysAimAndShootAtPlayer",
    story: "[Agent] always aims at player with animation and starts shooting",
    category: "Action", id: "ff484dcfaa9170c54238b5011ba6816a")]
public class AlwaysAimAndShootAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase _enemy;
    private bool _hasStartedAiming;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e == null || e.PlayerTransform == null) return Status.Failure;

        _hasStartedAiming = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;
        if (e.PlayerTransform == null || !e.State.PlayerDetected) return Status.Failure;

        if (e.State.IsAlertAnimationPlaying) return Status.Running;

        if (!_hasStartedAiming)
        {
            e.Navigation.Stop();
            e.Animator?.SetAiming(true);
        }

        Vector3 dir = (e.PlayerTransform.position - e.transform.position).normalized;
        dir.y = 0;
        Quaternion look = Quaternion.LookRotation(dir);
        e.transform.rotation = Quaternion.Slerp(e.transform.rotation, look,
            Time.deltaTime * e.Navigation.RotationSpeed);

        float angle = Quaternion.Angle(e.transform.rotation, look);
        if (angle < 10f && !_hasStartedAiming)
        {
            _hasStartedAiming = true;
            e.StartAttack();
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        var e = _enemy;
        if (e == null) return;

        if (e is RangedEnemy ranged)
            ranged.RangedCombat.StopFire();

        e.Animator?.SetAiming(false);
        e.Navigation.Resume();
    }
}