using Unity.Behavior;
using UnityEngine;
using System;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckPlayerVisible", story: "[EnemyAI] sees player", category: "Conditions", id: "6eaef50312d11f04e852c2b98ef9bb58")]
public partial class CheckPlayerVisibleCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    
    public override bool IsTrue()
    {
        if (EnemyAI.Value == null) return false;
        
        bool canSee = EnemyAI.Value.CanSeePlayer();
        
        if (canSee)
        {
            EnemyAI.Value.UpdateLastKnownPosition();
        }
        
        return canSee;
    }
}