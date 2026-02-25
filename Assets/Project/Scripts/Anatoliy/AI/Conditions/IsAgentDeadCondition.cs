using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentNOTDead", story: "Is [Agent] NOT Dead", category: "Conditions", id: "a580808ce3e84a63a7b605659e73325d")]
public partial class IsAgentDeadCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? (!enemy.State.IsDead) : false;
        return res;
    }
}
