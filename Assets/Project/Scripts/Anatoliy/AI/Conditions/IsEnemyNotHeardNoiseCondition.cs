using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Enemy NOT heard noise", story: "[EnemyAI] has NOT heard noise", category: "Conditions", id: "9f966c3a67ffc4276960b272df87cf31")]
public partial class IsEnemyNotHeardNoiseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return !EnemyAI.Value.heardNoise;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
