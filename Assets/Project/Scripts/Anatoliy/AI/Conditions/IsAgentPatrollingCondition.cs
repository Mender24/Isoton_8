using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentPatroling", story: "Is [Agent] patrolling", category: "Variable Conditions", id: "2adcedf558ff6ef0c15d49fb79154504")]
public partial class IsAgentPatrollingCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? enemy.ShouldPatrol : false;
        return res;
    }
}
