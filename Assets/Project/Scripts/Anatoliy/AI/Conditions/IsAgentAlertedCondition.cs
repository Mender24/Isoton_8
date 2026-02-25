using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentAlerted", story: "Is [Agent] alerted", category: "Variable Conditions", id: "a613cf166ea28b10c1de6b1ccc2b218b")]
public partial class IsAgentAlertedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? enemy.State.IsAlerted : false;
        return res;
    }
}
