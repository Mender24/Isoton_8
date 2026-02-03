using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttackPlayer", story: "[EnemyAI] attacks in melee", category: "Action", id: "98f53f4ecb43511e60125c3eca86d27c")]
public partial class MeleeAttackPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    private float _cachedSpeed = 0f;

    private bool _hasInitiatedAttack = false;
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null || EnemyAI.Value.playerTransform == null) 
            return Status.Failure;
        
        _hasInitiatedAttack = false;
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.playerTransform == null || !EnemyAI.Value.playerDetected) 
            return Status.Failure;
        
        // Поворачиваемся к игроку
        Vector3 direction = (EnemyAI.Value.playerTransform.position - EnemyAI.Value.transform.position).normalized;
        direction.y = 0;
        
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        EnemyAI.Value.transform.rotation = Quaternion.Slerp(
            EnemyAI.Value.transform.rotation,
            lookRotation,
            Time.deltaTime * EnemyAI.Value.rotationSpeed
        );
        
        float angleToTarget = Quaternion.Angle(EnemyAI.Value.transform.rotation, lookRotation);
        
        // Начинаем атаку когда повернулись к цели
        if (angleToTarget < 5f && !_hasInitiatedAttack && EnemyAI.Value.meleeAttackTimer <= 0 && EnemyAI.Value.IsInMeleeRange())
        {
            _hasInitiatedAttack = true;
            if (EnemyAI.Value.agent.isOnNavMesh)
                EnemyAI.Value.agent.isStopped = true;
            EnemyAI.Value.StartMeleeAttack();
        }
        
        // Продолжаем атаку пока идет анимация
        if (EnemyAI.Value.isMeleeAttacking)
        {
            return Status.Running;
        }

        // Проверяем, не вышел ли игрок из зоны атаки
        if (!EnemyAI.Value.IsInMeleeRange())
        {
            if (EnemyAI.Value.agent.isOnNavMesh)
                EnemyAI.Value.agent.isStopped = false;
            return Status.Failure;
        }

        // Если атака завершена
        if (_hasInitiatedAttack && !EnemyAI.Value.isMeleeAttacking)
        {
            if (EnemyAI.Value.agent.isOnNavMesh)
                EnemyAI.Value.agent.isStopped = false;
            return Status.Success;
        }
        
        return Status.Running;
    }
}

