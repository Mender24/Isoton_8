using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentRangedType", story: "[EnemyAI] is ranged type", category: "Variable Conditions", id: "0430c44660fbe25352f8a283fa473529")]
public partial class IsAgentRangedTypeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.combatType == global::EnemyAI.CombatType.Ranged;
    }
}
