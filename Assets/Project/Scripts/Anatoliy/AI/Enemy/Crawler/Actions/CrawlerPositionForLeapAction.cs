using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун занимает позицию засады вблизи игрока, оставаясь вне его поля зрения.
/// Success на позиции и не виден и готов к прыжку.
/// Failure заметили во время движения или не нашли позицию.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerPositionForLeap",
    story: "[Agent] positions for leap ambush",
    category: "Crawler",
    id: "c1a2b3d4e5f60003a1b2c3d4e5f60003")]
public partial class CrawlerPositionForLeapAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private CrawlerEnemy _crawler;
    private int          _repositionAttempts;
    private const int    MaxRepositionAttempts = 3;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        _repositionAttempts = 0;
        return TryMoveToAmbushSpot() ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        if (_crawler.WasRecentlyShot && _crawler.IsVisibleToPlayer())
            return Status.Failure;

        if (!_crawler.IsEnemyStopped()) return Status.Running;

        if (!_crawler.IsVisibleToPlayer()) return Status.Success;

        _repositionAttempts++;
        if (_repositionAttempts >= MaxRepositionAttempts) return Status.Failure;

        return TryMoveToAmbushSpot() ? Status.Running : Status.Failure;
    }

    protected override void OnEnd()
    {
        _crawler?.Navigation.Stop();
    }

    private bool TryMoveToAmbushSpot()
    {
        if (_crawler.TryFindAmbushPosition(out Vector3 ambush))
        {
            _crawler.Navigation.MoveTo(ambush, run: true);
            return true;
        }
        return false;
    }
}
