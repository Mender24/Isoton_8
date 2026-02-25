using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AgentLookAndCheckPlayer", story: "[Agent] looks and checks if player",
    category: "Action", id: "eab94d883aad2772ce487e67e7e7c09b")]
public partial class AgentSeesPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> SeesPlayer;

    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy == null) return Status.Failure;

        SeesPlayer.Value = _enemy.Perception.CanSeePlayer();

        if (SeesPlayer.Value)
        {
            _enemy.UpdateLastKnownPosition();
            return Status.Success;
        }

        if (_enemy is RangedEnemy)
            _enemy.State.PlayerDetected = false;

        return Status.Failure;
    }
}
