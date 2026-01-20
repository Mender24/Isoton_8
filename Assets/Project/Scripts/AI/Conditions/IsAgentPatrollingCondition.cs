using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentPatroling", story: "Is [enemyAI] patrolling", category: "Variable Conditions", id: "2adcedf558ff6ef0c15d49fb79154504")]
public partial class IsAgentPatrollingCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.shouldPatrol;
    }
}
