using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "ShouldTakeCover", story: "Should [Agent] take cover", category: "Conditions", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c305")]
public partial class ShouldTakeCoverCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyCoverModule _cover;

    public override bool IsTrue()
    {
        if (_cover == null && Agent.Value != null)
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();

        return _cover != null && _cover.ShouldTakeCover();
    }
}