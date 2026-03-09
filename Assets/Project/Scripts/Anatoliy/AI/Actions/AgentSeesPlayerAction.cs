// using System;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "AgentLookAndCheckPlayer", story: "[EnemyAI] looks and checks if player", category: "Action", id: "eab94d883aad2772ce487e67e7e7c09b")]
// public partial class AgentSeesPlayerAction : Action
// {
//     [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
//     [SerializeReference] public BlackboardVariable<bool> seesPlayer;

//     private bool _cachedSeesPlayer = false;
//     private float _timeBetweenUpdate = 0.5f;
//     private float _curTimer = 0f;

//     protected override Status OnStart()
//     {
//         return Status.Running;
//     }

//     protected override Status OnUpdate()
//     {
//         if (EnemyAI.Value == null) return Status.Failure;
        
//         _cachedSeesPlayer = seesPlayer.Value;
//         seesPlayer.Value = EnemyAI.Value.CanSeePlayer();

//         if (seesPlayer.Value)
//         {
//             EnemyAI.Value.UpdateLastKnownPosition();
//             return Status.Success;
//         }
            
//         if (EnemyAI.Value.combatType == global::EnemyAI.CombatType.Ranged)
//             EnemyAI.Value.playerDetected = false;
//         return Status.Failure;
//     }
// }

