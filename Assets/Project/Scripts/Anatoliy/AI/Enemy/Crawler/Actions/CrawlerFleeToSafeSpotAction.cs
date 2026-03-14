using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун убегает в безопасное место, не видимое игроку.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerFleeToSafeSpot",
    story: "[Agent] flees to a safe spot",
    category: "Crawler",
    id: "c1a2b3d4e5f60002a1b2c3d4e5f60002")]
public partial class CrawlerFleeToSafeSpotAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SafeSpotRadius   = new(12f);
    [SerializeReference] public BlackboardVariable<bool>  ForceAlways      = new(false);
    [SerializeReference] public BlackboardVariable<bool>  PreferWallCeiling = new(false);

    private CrawlerEnemy _crawler;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        if (!ForceAlways.Value && (!_crawler.WasRecentlyShot || !_crawler.IsVisibleToPlayer()))
            return Status.Failure;

        Vector3 dest;
        bool found;

        if (PreferWallCeiling.Value)
        {
            found = _crawler.TryFindWallCeilingSafeSpot(out dest);
        }
        else
        {
            found = _crawler.TryFindSafeNavPoint(
                _crawler.transform.position, SafeSpotRadius.Value, out dest);

            if (!found)
                found = _crawler.Navigation.TryGetRandomNavPoint(
                    _crawler.transform.position, SafeSpotRadius.Value, out dest);
        }

        if (!found) return Status.Failure;

        _crawler.Navigation.MoveTo(dest, run: true);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;
        return _crawler.IsEnemyStopped() ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        _crawler?.Navigation.Stop();
    }
}
