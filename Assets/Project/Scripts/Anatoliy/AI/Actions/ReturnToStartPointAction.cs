using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReturnToStartPoint", story: "[EnemyAI] returns to start point if it exists", category: "Action", id: "d1381cb7318f366f389aa132e65d2093")]
public partial class ReturnToStartPointAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    protected override Status OnStart()
    {
        
        if (EnemyAI.Value.startPosition != Vector3.zero)
        {
            if (EnemyAI.Value.agent.pathEndPosition != EnemyAI.Value.startPosition)
            {
                EnemyAI.Value.agent.speed = EnemyAI.Value.walkSpeed;
                EnemyAI.Value.agent.isStopped = false;
                EnemyAI.Value.agent.SetDestination(EnemyAI.Value.startPosition);
            }
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.playerDetected)
            return Status.Failure;

        if (EnemyAI.Value.agent.remainingDistance <= EnemyAI.Value.agent.stoppingDistance) // FIXME: smh this always throw true, idk y, need logic to end only when finished route
        {
            // EnemyAI.Value.startPosition = Vector3.zero;
            // EnemyAI.Value.agent.ResetPath();
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

