using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentNotAlerted", story: "Is [EnemyAI] NOT alerted", category: "Variable Conditions", id: "a613cf166ea28b10c1d1eb1ccc2b218b")]
public partial class IsAgentNotAlertedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return !EnemyAI.Value.isAlerted;
    }
}
