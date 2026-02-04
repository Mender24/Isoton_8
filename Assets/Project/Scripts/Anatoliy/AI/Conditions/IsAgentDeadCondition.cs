using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAgentNOTDead", story: "Is [EnemyAI] NOT Dead", category: "Conditions", id: "a580808ce3e84a63a7b605659e73325d")]
public partial class IsAgentDeadCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return !EnemyAI.Value.isDead;
    }
}
