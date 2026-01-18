using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentAlerted", story: "Is [EnemyAI] alerted", category: "Variable Conditions", id: "a613cf166ea28b10c1de6b1ccc2b218b")]
public partial class IsAgentAlertedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.isAlerted;
    }
}
