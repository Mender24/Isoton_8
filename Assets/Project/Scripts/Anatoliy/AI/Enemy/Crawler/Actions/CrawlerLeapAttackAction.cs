using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

/// <summary>
/// Ползун прыгает к игроку по параболической дуге, затем атакует.
/// Во время прыжка NavMeshAgent.updatePosition = false,
/// CrawlerSurfaceAligner.IsActive = false.
/// После приземления оба восстанавливаются через Agent.Warp().
/// </summary>
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CrawlerLeapAttack",
    story: "[Agent] leaps and attacks player",
    category: "Crawler",
    id: "c1a2b3d4e5f60004a1b2c3d4e5f60004")]
public partial class CrawlerLeapAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> LeapDuration  = new(0.45f);
    [SerializeReference] public BlackboardVariable<float> LeapArcHeight = new(2.5f);
    [SerializeReference] public BlackboardVariable<float> AlignDuration = new(0.15f);

    private enum Phase { Leaping, Aligning, Attacking, Done }

    private CrawlerEnemy _crawler;
    private Phase        _phase;
    private float        _elapsed;
    private Vector3      _leapStart;
    private Vector3      _leapEnd;
    private Quaternion   _alignStartRot;
    private bool         _attackStarted;

    protected override Status OnStart()
    {
        if (_crawler == null)
            _crawler = Agent.Value.GetComponent<CrawlerEnemy>();

        if (_crawler == null || _crawler.State.IsDead || _crawler.PlayerTransform == null)
            return Status.Failure;

        _phase         = Phase.Leaping;
        _elapsed       = 0f;
        _attackStarted = false;
        _leapStart     = _crawler.transform.position;
        _leapEnd       = _crawler.FindLandingPoint();

        _crawler.Navigation.Agent.isStopped      = true;
        _crawler.Navigation.Agent.updatePosition = false;
        _crawler.SurfaceAlignmentActive          = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_crawler == null || _crawler.State.IsDead) return Status.Failure;

        _elapsed += Time.deltaTime;

        return _phase switch
        {
            Phase.Leaping   => TickLeaping(),
            Phase.Aligning  => TickAligning(),
            Phase.Attacking => TickAttacking(),
            _               => Status.Success,
        };
    }

    protected override void OnEnd()
    {
        if (_crawler != null)
        {
            _crawler.Navigation.Agent.Warp(_crawler.transform.position);
            _crawler.Navigation.Agent.updatePosition = true;
            _crawler.Navigation.Agent.isStopped      = false;
        }
        if (_crawler != null) _crawler.SurfaceAlignmentActive = true;
    }

    private Status TickLeaping()
    {
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

        if (t >= 1f)
        {
            _crawler.transform.position = _leapEnd;
            _phase   = Phase.Aligning;
            _elapsed = 0f;
            _alignStartRot = _crawler.transform.rotation;
        }

        return Status.Running;
    }

    private Status TickAligning()
    {
        float t = Mathf.Clamp01(_elapsed / AlignDuration.Value);

        Vector3 toPlayer = _crawler.PlayerTransform != null
            ? (_crawler.PlayerTransform.position - _crawler.transform.position).normalized
            : _crawler.transform.forward;
        toPlayer.y = 0f;

        Quaternion targetRot = toPlayer.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(toPlayer, Vector3.up)
            : Quaternion.identity;

        _crawler.transform.rotation = Quaternion.Slerp(_alignStartRot, targetRot, t);

        if (t >= 1f)
        {
            _phase         = Phase.Attacking;
            _elapsed       = 0f;
            _attackStarted = false;

            if (_crawler != null) _crawler.SurfaceAlignmentActive = true;
        }

        return Status.Running;
    }

    private Status TickAttacking()
    {
        if (!_attackStarted)
        {
            _crawler.State.PlayerDetected = true;
            if (_crawler.MeleeCombat.CanAttack)
            {
                _crawler.MeleeCombat.StartAttack();
                _attackStarted = true;
            }
            else
            {
                _phase = Phase.Done;
                return Status.Running;
            }
        }

        if (!_crawler.State.IsMeleeAttacking)
            _phase = Phase.Done;

        return _phase == Phase.Done ? Status.Success : Status.Running;
    }
}
