using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун стоит на месте случайное время перед следующим циклом.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerWait",
    story: "[Agent] waits [MinTime]-[MaxTime] seconds",
    category: "Crawler",
    id: "c1a2b3d4e5f60005a1b2c3d4e5f600aa")]
public partial class CrawlerWaitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> MinTime = new(3f);
    [SerializeReference] public BlackboardVariable<float> MaxTime = new(7f);

    private CrawlerEnemy _crawler;
    private float        _waitDuration;
    private float        _elapsed;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        _crawler.Navigation.Stop();
        _waitDuration = UnityEngine.Random.Range(MinTime.Value, MaxTime.Value);
        _elapsed      = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;
        _elapsed += Time.deltaTime;
        return _elapsed >= _waitDuration ? Status.Success : Status.Running;
    }
}
