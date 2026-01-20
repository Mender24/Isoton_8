using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AgentLookAndCheckPlayer", story: "[EnemyAI] looks and checks if player", category: "Action", id: "eab94d883aad2772ce487e67e7e7c09b")]
public partial class AgentSeesPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    [SerializeReference] public BlackboardVariable<bool> seesPlayer;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (EnemyAI.Value == null) return Status.Failure;
        
        seesPlayer.Value = EnemyAI.Value.CanSeePlayer();
        
        if (seesPlayer.Value)
        {
            EnemyAI.Value.UpdateLastKnownPosition();
            return Status.Success;
        }
            
        EnemyAI.Value.playerDetected = false;
        return Status.Failure;
    }
}

