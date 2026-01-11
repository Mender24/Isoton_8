using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SearchPlayer", story: "[EnemyAI] searches player", category: "Action", id: "ff484dcfaa9e70c54778b6355ba6816a")]
public partial class SearchPatternNode : Action
{
    [SerializeReference] public BlackboardVariable<EnemyAI> EnemyAI;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new BlackboardVariable<float>(10f);
    
    private Vector3 searchPoint;
    private int searchAttempts = 0;
    private const int maxSearchAttempts = 3;
    
    protected override Status OnStart()
    {
        if (EnemyAI.Value == null) return Status.Failure;
        
        searchAttempts = 0;
        GenerateNewSearchPoint();
        EnemyAI.Value.isSearching = true;
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        if (EnemyAI.Value.agent.remainingDistance <= EnemyAI.Value.agent.stoppingDistance)
        {
            searchAttempts++;
            
            if (searchAttempts >= maxSearchAttempts)
            {
                EnemyAI.Value.isSearching = false;
                return Status.Success;
            }
            
            GenerateNewSearchPoint();
        }

        if (EnemyAI.Value.playerDetected)
        {
            if (EnemyAI.Value.agent.isOnNavMesh)
                EnemyAI.Value.agent.ResetPath();
            EnemyAI.Value.isSearching = false;
            return Status.Failure;
        }
        
        return Status.Running;
    }
    
    private void GenerateNewSearchPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * SearchRadius.Value;
        randomDirection += EnemyAI.Value.lastKnownPlayerPosition;
        
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, SearchRadius.Value, 1))
        {
            searchPoint = hit.position;
            EnemyAI.Value.agent.SetDestination(searchPoint);
            if (EnemyAI.Value.animator) EnemyAI.Value.animator?.SetBool("Walking", true);
        }
    }
    
    protected override void OnEnd()
    {
        EnemyAI.Value.isSearching = false;
        if (EnemyAI.Value.animator) EnemyAI.Value.animator?.SetBool("Walking", false);
    }
}