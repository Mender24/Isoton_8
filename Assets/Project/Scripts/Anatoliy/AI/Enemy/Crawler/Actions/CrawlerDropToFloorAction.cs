using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун прыгает с потолка/стены на пол перед игроком
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerDropToFloor",
    story: "[Agent] drops from ceiling to floor before player",
    category: "Crawler",
    id: "c1a2b3d4e5f60005a1b2c3d4e5f60005")]
public partial class CrawlerDropToFloorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> LeapDuration  = new(0.45f);
    [SerializeReference] public BlackboardVariable<float> LeapArcHeight = new(1.5f);

    private CrawlerEnemy _crawler;
    private float        _elapsed;
    private Vector3      _leapStart;
    private Vector3      _leapEnd;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        if (!_crawler.TryFindDropZoneBeforePlayer(out Vector3 dropZone))
            return Status.Failure;

        _elapsed   = 0f;
        _leapStart = _crawler.transform.position;
        _leapEnd   = dropZone;

        _crawler.Navigation.Agent.isStopped      = true;
        _crawler.Navigation.Agent.updatePosition = false;
        _crawler.SurfaceAlignmentActive          = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / LeapDuration.Value);

        Vector3 pos = Vector3.Lerp(_leapStart, _leapEnd, t);
        pos += Vector3.up * (Mathf.Sin(t * Mathf.PI) * LeapArcHeight.Value);
        _crawler.transform.position = pos;

        Vector3 dir = (_leapEnd - _leapStart).normalized;
        if (dir.sqrMagnitude > 0.01f)
        {
            _crawler.transform.rotation = Quaternion.Slerp(
                _crawler.transform.rotation,
                Quaternion.LookRotation(dir, Vector3.up),
                20f * Time.deltaTime);
        }

        if (t < 1f) return Status.Running;

        _crawler.transform.position = _leapEnd;
        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (_crawler == null) return;
        _crawler.Navigation.Agent.Warp(_crawler.transform.position);
        _crawler.Navigation.Agent.updatePosition = true;
        _crawler.Navigation.Agent.isStopped      = false;
        _crawler.SurfaceAlignmentActive          = true;
    }
}
