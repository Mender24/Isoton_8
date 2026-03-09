// using System;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "WanderAround", story: "[EnemyAI] wanders around", category: "Action", id: "ff484dcfaa1170c54778b6355ba6816a")]
// public partial class WanderAroundNode : Action
// {
//     [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
//     [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(5f);
//     [SerializeReference] public BlackboardVariable<float> stoppingDistance = new BlackboardVariable<float>(3f);
    
//     private Vector3 _searchPoint;
//     private int _searchAttempts = 0;
//     private const int _maxSearchAttempts = 3;
//     private float _cacheStoppingDistance = 1f;
//     private float _cacheSpeed = 5f;
    
//     protected override Status OnStart()
//     {
//         if (EnemyAI.Value == null || EnemyAI.Value.isDead) return Status.Failure;
        
//         _searchAttempts = 0;
//         if (!EnemyAI.Value.agent.hasPath)
//             GenerateNewSearchPoint();

//         _cacheStoppingDistance = EnemyAI.Value.agent.stoppingDistance;
//         EnemyAI.Value.agent.stoppingDistance = stoppingDistance.Value;
//         _cacheSpeed = EnemyAI.Value.agent.speed;

//         EnemyAI.Value.isSearching = true;

//         EnemyAI.Value.animationController?.PlaySearch();
        
//         return Status.Running;
//     }
    
//     protected override Status OnUpdate()
//     {
//         if (EnemyAI.Value.isReload)
//         {
//             EnemyAI.Value.agent.speed = 0;
//         }
//         else
//             EnemyAI.Value.agent.speed = _cacheSpeed;

//         if (EnemyAI.Value.agent.remainingDistance <= EnemyAI.Value.agent.stoppingDistance)
//         {
//             _searchAttempts++;
            
//             if (_searchAttempts >= _maxSearchAttempts)
//             {
//                 EnemyAI.Value.isSearching = false;
//                 EnemyAI.Value.agent.stoppingDistance = _cacheStoppingDistance;
//                 EnemyAI.Value.agent.speed = _cacheSpeed;
//                 return Status.Success;
//             }
            
//             GenerateNewSearchPoint();
//         }

//         if (EnemyAI.Value.playerDetected)
//         {
//             if (EnemyAI.Value.agent.isOnNavMesh)
//                 EnemyAI.Value.agent.ResetPath();
//             EnemyAI.Value.isSearching = false;
//             EnemyAI.Value.agent.stoppingDistance = _cacheStoppingDistance;
//             EnemyAI.Value.agent.speed = _cacheSpeed;
//             return Status.Failure;
//         }
        
//         return Status.Running;
//     }
    
//     private void GenerateNewSearchPoint()
//     {
//         Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * SearchRadius.Value;
//         randomDirection += EnemyAI.Value.transform.position;
        
//         int attempts = 0;
//         while (attempts < 10)
//         {
//             UnityEngine.AI.NavMeshHit hit;
//             if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, SearchRadius.Value, 1))
//             {
//                 _searchPoint = hit.position;

//                 UnityEngine.AI.NavMeshPath path = new();
//                 if (EnemyAI.Value.agent.CalculatePath(_searchPoint, path))
//                 {
//                     EnemyAI.Value.agent.SetDestination(_searchPoint);
//                     return;
//                 }
//             }
//             attempts++;
//         }
//     }
    
//     protected override void OnEnd()
//     {
//         EnemyAI.Value.isSearching = false;
//         EnemyAI.Value.agent.speed = _cacheSpeed;
//     }
// }