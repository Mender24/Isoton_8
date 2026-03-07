// using System;
// using Unity.Behavior;
// using UnityEngine;
// using Action = Unity.Behavior.Action;
// using Unity.Properties;

// [Serializable, GeneratePropertyBag]
// [NodeDescription(name: "TryToHear", story: "[EnemyAI] checks if hears anything", category: "Action", id: "8035a35a8d131d5125a63c856e0f5d9f")]
// public partial class TryToHearAction : Action
// {
//     [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

//     protected override Status OnStart()
//     {
//         return Status.Running;
//     }

//     protected override Status OnUpdate()
//     {
//         if (!EnemyAI.Value) return Status.Failure;

//         return EnemyAI.Value.OnNoiseDetected();
//     }

//     protected override void OnEnd()
//     {
//     }
// }

