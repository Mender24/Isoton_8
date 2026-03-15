using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ThrowGrenadeAtPlayer",
    story: "[Agent] throws grenade at player",
    category: "Action", id: "c3a21d9fb7e14c8f3a9d5b01e2f74d8a")]
public partial class ThrowGrenadeAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase _enemy;
    private GrenadeThrowModule _grenadeModule;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null || _enemy.PlayerTransform == null) return Status.Failure;

        _grenadeModule = _enemy.GetComponent<GrenadeThrowModule>();
        if (_grenadeModule == null || !_grenadeModule.CanThrowGrenade) return Status.Failure;

        _enemy.State.ShouldThrowGrenade = false;
        _enemy.Navigation.Stop();
        _enemy.Animator?.SetAiming(false);
        _grenadeModule.StartWindUp();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!_enemy.State.PlayerDetected) return Status.Failure;

        RotateTowardsPlayer();

        // Module went back to Idle. Either throw completed or was cancelled by hit reaction
        if (_grenadeModule.Phase == GrenadeThrowPhase.Idle)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _grenadeModule?.Cancel();

        _enemy?.Navigation.Resume();
        _enemy?.Animator?.SetAiming(true);
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
