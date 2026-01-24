using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is EnemyAI has heard noise", story: "[EnemyAI] has heard noise", category: "Conditions", id: "5363d05d61435a0fc1cf91bcd16461d6")]
public partial class IsAgentHasHeardNoiseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.heardNoise;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
