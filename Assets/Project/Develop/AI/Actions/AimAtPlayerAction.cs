using Unity.Behavior;
using System;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AimAtPlayer", story: "[EnemyAI] aims at player", category: "Action/Animation", id: "ff484dcfaa9e70c54238b5055ba6816a")]
public class AimAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    private Vector3 lastFrameRotDirection = new Vector3();
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null || EnemyAI.Value.playerTransform == null) 
            return Status.Failure;
        
        // Останавливаем движение
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
            return Status.Success;

        // Здесь будет логика стрельбы
        EnemyAI.Value.StartFire();
        // ------
        
        lastFrameRotDirection = direction;
        return Status.Running;
    }
    
    protected override void OnEnd()
    {
        EnemyAI.Value.agent.isStopped = false;
        if (EnemyAI.Value.animator) EnemyAI.Value.animator?.SetBool("Aiming", false);
    }
}