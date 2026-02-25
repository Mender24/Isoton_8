using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentActive", story: "Is [Agent] activated", category: "Variable Conditions", id: "a613cf166ea28b1111de6b1ccc2b218b")]
public partial class IsAgentActiveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? enemy.State.IsActivated : false;
        return res;
    }
}
