using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToNoise", story: "[EnemyAI] moves to the heard noise", category: "Action", id: "9e7be751201f2bdd0fbec0f801b19bf1")]
public partial class MoveToNoiseAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    protected override Status OnStart()
    {
        if (EnemyAI.Value == null || EnemyAI.Value.agent == null)
            return Status.Failure;

        Vector3 targetPosition = EnemyAI.Value.GetNoiseInvestigationTarget();
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, Vector3.down, out hit, 100))
        {
            targetPosition.y = hit.point.y;
        }
        EnemyAI.Value.agent.SetDestination(targetPosition);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!EnemyAI.Value.heardNoise || EnemyAI.Value.playerDetected)
            return Status.Failure;

        if (EnemyAI.Value.agent.remainingDistance <= EnemyAI.Value.agent.stoppingDistance)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (EnemyAI.Value.agent != null && EnemyAI.Value.agent.isOnNavMesh)
        {
            EnemyAI.Value.agent.ResetPath();
        }
    }
}

