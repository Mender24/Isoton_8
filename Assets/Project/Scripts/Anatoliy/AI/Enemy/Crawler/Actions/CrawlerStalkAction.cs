using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун перемещается по поверхностям, избегая зоны видимости игрока.
///
/// PreferCeiling = true:
///   Целенаправленно ищет точки на стенах/потолке рядом с игроком.
///   Success = занял позицию на потолке/стене близко к игроку и готов к прыжку вниз.
///   Failure = заметили+стреляли, или не смог выйти на потолок за MaxWaypoints попыток.
///
/// PreferCeiling = false:
///   Прячется за стены и углы.
///   Success = отсиделся N точек и готов к атаке с пола.
///   Failure = заметили+стреляли.
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerStalk",
    story: "[Agent] stalks along surfaces avoiding player",
    category: "Crawler",
    id: "c1a2b3d4e5f60001a1b2c3d4e5f60001")]
public partial class CrawlerStalkAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SafeSpotRadius    = new(12f);
    [SerializeReference] public BlackboardVariable<bool>  PreferCeiling     = new(true);
    /// <summary>Сколько точек посетить при PreferCeiling=true прежде чем сдаться.</summary>
    [SerializeReference] public BlackboardVariable<int>   MaxWaypoints      = new(6);
    /// <summary>Сколько точек посетить при PreferCeiling=false прежде чем атаковать с пола.</summary>
    [SerializeReference] public BlackboardVariable<int>   FloorHideWaypoints = new(2);

    private CrawlerEnemy _crawler;
    private int          _waypointsVisited;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        _waypointsVisited = 0;
        MoveToNextTarget();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;
        if (_crawler.WasRecentlyShot && _crawler.IsVisibleToPlayer()) return Status.Failure;
        if (!_crawler.IsEnemyStopped()) return Status.Running;

        _waypointsVisited++;

        if (PreferCeiling.Value)
        {
            if (_crawler.IsPositionedOnCeilingWallForAttack())
                return Status.Success;

            if (_waypointsVisited >= MaxWaypoints.Value)
                return Status.Failure;
        }
        else
        {
            if (_waypointsVisited >= FloorHideWaypoints.Value)
                return Status.Success;
        }

        MoveToNextTarget();
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _crawler?.Navigation.Stop();
    }

    private void MoveToNextTarget()
    {
        if (PreferCeiling.Value)
        {
            if (_crawler.TryFindCeilingWallNearPlayer(out Vector3 ceiling))
            {
                _crawler.Navigation.MoveTo(ceiling, run: false);
                return;
            }
        }

        if (_crawler.TryFindSafeNavPoint(
            _crawler.transform.position, SafeSpotRadius.Value, out Vector3 safe))
        {
            _crawler.Navigation.MoveTo(safe, run: false);
            return;
        }

        if (_crawler.Navigation.TryGetRandomNavPoint(
            _crawler.transform.position, SafeSpotRadius.Value, out Vector3 random))
        {
            _crawler.Navigation.MoveTo(random, run: false);
        }
    }
}
