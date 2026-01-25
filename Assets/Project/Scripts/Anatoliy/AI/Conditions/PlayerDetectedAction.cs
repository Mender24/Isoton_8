using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerDetected", story: "[EnemyAI] on player detected", category: "Action", id: "8725f4fdd3f7bb34cbcd9a1ceb49fff8")]
public partial class PlayerDetectedAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!EnemyAI.Value) return Status.Failure;
        return EnemyAI.Value.OnPlayerDetected();
    }

    protected override void OnEnd()
    {
    }
}

