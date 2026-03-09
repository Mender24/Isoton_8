using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsCoverBlown", story: "Is [Agent] cover blown", category: "Conditions", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c304")]
public partial class IsCoverBlownCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyCoverModule _cover;

    public override bool IsTrue()
    {
        if (_cover == null && Agent.Value != null)
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();

        return _cover != null && _cover.IsCoverBlown();
    }
}