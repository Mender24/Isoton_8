using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChasePlayer", story: "[EnemyAI] chases player", category: "Action", id: "a1d208cde47eb49f2e8a401e2af81d95")]
public partial class ChasePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null || EnemyAI.Value.playerTransform == null)
            return Status.Failure;
        
        EnemyAI.Value.agent.speed = EnemyAI.Value.runSpeed;
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (!EnemyAI.Value.playerDetected || EnemyAI.Value.playerTransform == null)
            return Status.Failure;
        
        // Обновляем путь к игроку
        if (EnemyAI.Value.agent.isOnNavMesh)
        {
            EnemyAI.Value.agent.SetDestination(EnemyAI.Value.playerTransform.position);
        }
        
        // Если дошли до дистанции атаки - успех
        if (EnemyAI.Value.IsInMeleeRange())
        {
            return Status.Success;
        }
        
        return Status.Running;
    }
    
    protected override void OnEnd()
    {
        if (EnemyAI.Value?.agent != null && EnemyAI.Value.agent.isOnNavMesh)
        {
            EnemyAI.Value.agent.ResetPath();
        }
    }
}

