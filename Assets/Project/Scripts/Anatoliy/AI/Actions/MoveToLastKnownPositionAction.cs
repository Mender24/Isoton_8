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
        Vector3 vecWithNoY = new Vector3(EnemyAI.Value.lastKnownPlayerPosition.x, GameObject.transform.position.y, EnemyAI.Value.lastKnownPlayerPosition.z);
        EnemyAI.Value.agent.SetDestination(vecWithNoY);
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.isReload)
            EnemyAI.Value.agent.speed = 0;
        else
            EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;

        if (EnemyAI.Value.agent.hasPath) // FIXME: smh this always throw false, idk y, need logic to end only when finished route
        {
            return Status.Running;
        }
        
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
        return Status.Success;
    }

    protected override void OnEnd()
    {
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
    }
}

