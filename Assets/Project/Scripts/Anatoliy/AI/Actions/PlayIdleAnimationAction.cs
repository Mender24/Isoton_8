// using System;
// using System.Collections.Generic;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "PlayIdleAnimation", story: "[EnemyAI] plays random [IdleAnimations]", category: "Action/Animation", id: "ff484dcfaa9e70c54778b1255ba6816a")]
// public partial class PlayIdleAnimationAction : Action
// {
//     [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
//     [SerializeReference] public BlackboardVariable<List<string>> IdleAnimations;

//     protected override Status OnStart()
//     {
//         if (EnemyAI.Value?.animationController == null) return Status.Failure;
        
//         // Не актуально
//         // if (IdleAnimations.Value != null && IdleAnimations.Value.Count > 0)
//         // {
//         //     string randomIdle = IdleAnimations.Value[UnityEngine.Random.Range(0, IdleAnimations.Value.Count)];
//         //     EnemyAI.Value.animationController.Play(randomIdle);
//         // }
//         // else
//         // {
//         //     EnemyAI.Value.animationController.Play("Idle");
//         // }
        
//         return Status.Success;
//     }

//     protected override Status OnUpdate()
//     {
//         return Status.Success;
//     }

//     protected override void OnEnd()
//     {
//     }
// }

