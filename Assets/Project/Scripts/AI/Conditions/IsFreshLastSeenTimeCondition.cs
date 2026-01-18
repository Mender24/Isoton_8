using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "LastSeenTimeIsFresh", story: "[EnemyAI] 's last seen time is fresh", category: "Variable Conditions", id: "2af9cc9a0b9cf5baf6618f97d8e50032")]
public partial class IsFreshLastSeenTimeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;

    public override bool IsTrue()
    {
        return EnemyAI.Value.timeSinceLastSeen < EnemyAI.Value.forgetTime;
    }
}
