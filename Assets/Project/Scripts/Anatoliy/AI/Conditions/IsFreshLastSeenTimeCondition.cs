using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "LastSeenTimeIsFresh", story: "[Agent] 's last seen time is fresh", category: "Variable Conditions", id: "2af9cc9a0b9cf5baf6618f97d8e50032")]
public partial class IsFreshLastSeenTimeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    public override bool IsTrue()
    {
        EnemyBase enemy;
        bool res = Agent.Value.TryGetComponent(out enemy) ? (enemy.State.TimeSinceLastSeen > 0 && enemy.State.TimeSinceLastSeen < enemy.Perception.ForgetTime) : false;
        return res;
    }
}
