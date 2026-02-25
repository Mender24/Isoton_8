// using System;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "OnPlayerDetectedTrigger", story: "[EnemyAI] plays detection animation", category: "Action", id: "ff484dcfaa9e70c54778b7755ba6816a")]
// public partial class OnPlayerDetectedAction : Action
// {
//     [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    
//     protected override Status OnStart()
//     {
//         if (EnemyAI.Value == null) return Status.Failure;
        
//         EnemyAI.Value.animationController?.PlayAlert();
        
//         return Status.Success;
//     }
// }