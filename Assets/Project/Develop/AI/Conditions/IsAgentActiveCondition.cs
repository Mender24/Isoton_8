using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentActive", story: "Is [EnemyAI] activated", category: "Variable Conditions", id: "a613cf166ea28b1111de6b1ccc2b218b")]
public partial class IsAgentActiveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.isActivated;
    }
}
