using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Agent NOT heard noise", story: "[Agent] has NOT heard noise", category: "Conditions", id: "9f966c3a67ffc4276960b272df87cf31")]
public partial class IsEnemyNotHeardNoiseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? (!enemy.State.HeardNoise) : false;
        return res;
    }
}
