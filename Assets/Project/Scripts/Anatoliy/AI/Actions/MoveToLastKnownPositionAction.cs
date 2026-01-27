using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToLastKnownPosition", story: "[EnemyAI] moves to last known position", category: "Action", id: "ff484dcfaa9e11c54778b5055ba6816a")]
public partial class MoveToLastKnownPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null)
            return Status.Failure;
        
        EnemyAI.Value.agent.ResetPath();
        EnemyAI.Value.agent.SetDestination(EnemyAI.Value.lastKnownPlayerPosition);
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.isReload)
            EnemyAI.Value.agent.speed = 0;
        else
            EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;

        if (EnemyAI.Value.agent.remainingDistance <= EnemyAI.Value.agent.stoppingDistance)
        {
            EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
            return Status.Success;
        }
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
    }
}

