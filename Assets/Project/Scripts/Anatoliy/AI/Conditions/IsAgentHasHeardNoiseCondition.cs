using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Agent has heard noise", story: "[Agent] has heard noise", category: "Conditions", id: "5363d05d61435a0fc1cf91bcd16461d6")]
public partial class IsAgentHasHeardNoiseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? enemy.State.HeardNoise : false;
        return res;
    }
}
