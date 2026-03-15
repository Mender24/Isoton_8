using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentNotAlerted", story: "Is [Agent] NOT alerted", category: "Variable Conditions", id: "a613cf166ea28b10c1d1eb1ccc2b218b")]
public partial class IsAgentNotAlertedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? (!enemy.State.IsAlerted) : false;
        return res;
    }
}
