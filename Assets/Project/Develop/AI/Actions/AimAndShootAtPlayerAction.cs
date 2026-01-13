using Unity.Behavior;
using System;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AimAndShootAtPlayer", story: "[EnemyAI] aims at player with animation and starts shooting", category: "Action", id: "ff484dcfaa9e70c54238b5011ba6816a")]
public class AimAndShooAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    private Vector3 lastFrameRotDirection = new Vector3();
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null || EnemyAI.Value.playerTransform == null) 
            return Status.Failure;
        
        if (EnemyAI.Value.agent.isOnNavMesh)
            EnemyAI.Value.agent.isStopped = true;
            
        if (EnemyAI.Value.animator)
        {
            EnemyAI.Value.animator?.SetBool("Walking", false);
            EnemyAI.Value.animator?.SetBool("Running", false);
            EnemyAI.Value.animator?.SetBool("Aiming", true);
        }

        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.playerTransform == null) return Status.Failure;
        
        if (!EnemyAI.Value.playerDetected) return Status.Failure;

        // Поворот к игроку
        Vector3 direction = (EnemyAI.Value.playerTransform.position - EnemyAI.Value.transform.position).normalized;
        direction.y = 0;
        
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        EnemyAI.Value.transform.rotation = Quaternion.Slerp(
            EnemyAI.Value.transform.rotation,
            lookRotation,
            Time.deltaTime * EnemyAI.Value.rotationSpeed
        );
        
        if (direction == lastFrameRotDirection)
        {
            EnemyAI.Value.StartFire();
        }
        
        if (EnemyAI.Value.isFire)
        {
            return Status.Running;
        }
        
        lastFrameRotDirection = direction;
        return Status.Running;
    }
    
    protected override void OnEnd()
    {
        if (EnemyAI.Value.agent.isOnNavMesh)
            EnemyAI.Value.agent.isStopped = false;
        if (EnemyAI.Value.animator) EnemyAI.Value.animator?.SetBool("Aiming", false);
    }
}