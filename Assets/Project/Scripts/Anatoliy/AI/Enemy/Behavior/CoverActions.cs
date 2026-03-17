using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindCover", story: "[Agent] searches for cover",
    category: "Action/Cover", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c301")]
public partial class FindCoverAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase          _enemy;
    private EnemyCoverModule   _cover;

    protected override Status OnStart()
    {
        if (_enemy == null)
        {
            _enemy = Agent.Value.GetComponent<EnemyBase>();
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();
        }

        if (_enemy == null || _cover == null) return Status.Failure;
        if (_enemy.State.IsDead) return Status.Failure;

        _enemy.State.IsSearching = false;

        bool found = _cover.FindAndOccupyCover();
        return found ? Status.Success : Status.Failure;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToCover", story: "[Agent] moves to cover point",
    category: "Action/Cover", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c302")]
public partial class MoveToCoverAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase        _enemy;
    private EnemyCoverModule _cover;

    protected override Status OnStart()
    {
        if (_enemy == null)
        {
            _enemy = Agent.Value.GetComponent<EnemyBase>();
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();
        }

        if (_enemy == null || _cover == null)      return Status.Failure;
        if (_enemy.State.IsDead)                   return Status.Failure;
        if (!_enemy.State.HasCover)                return Status.Failure;

        _enemy.Navigation.MoveTo(_enemy.State.CurrentCoverPoint, run: true);
        _enemy.State.IsMovingToCover = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.IsDead)           return Status.Failure;
        if (!_enemy.State.PlayerDetected)  return Status.Failure;

        if (_cover.IsAtCoverPoint())
        {
            _enemy.State.IsMovingToCover = false;
            _enemy.State.IsInCover = true;
            _enemy.Navigation.Stop();
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _enemy.State.IsMovingToCover = false;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitInCover", story: "[Agent] waits in cover for [WaitTime] seconds",
    category: "Action/Cover", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c303")]
public partial class WaitInCoverAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> WaitTime    = new(2f);
    [SerializeReference] public BlackboardVariable<float> ReturnFireWindow = new(1.5f);

    private EnemyBase        _enemy;
    private EnemyCoverModule _cover;
    private float            _elapsed;

    protected override Status OnStart()
    {
        if (_enemy == null)
        {
            _enemy = Agent.Value.GetComponent<EnemyBase>();
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();
        }

        if (_enemy == null || _cover == null) return Status.Failure;
        if (_enemy.State.IsDead)              return Status.Failure;

        _elapsed = 0f;
        _enemy.Navigation.Stop();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.IsDead)          return Status.Failure;
        if (!_enemy.State.PlayerDetected) return Status.Failure;

        // Игрок выстрелил в бота — ответный огонь
        if (Time.time - _enemy.State.LastDamageTime <= ReturnFireWindow.Value)
        {
            _enemy.State.IsInCover = false;
            return Status.Success;
        }

        if (_cover.IsCoverBlown())
        {
            _enemy.State.IsInCover = false;
            return Status.Success;
        }

        _elapsed += Time.deltaTime;
        if (_elapsed >= WaitTime.Value)
        {
            _enemy.State.IsInCover = false;
            return Status.Success;
        }

        return Status.Running;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToAttackPosition", story: "[Agent] moves to attack position (peek from cover)",
    category: "Action/Cover", id: "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c306")]
public partial class MoveToAttackPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> PeekTimeout = new(2f);

    private EnemyBase        _enemy;
    private EnemyCoverModule _cover;
    private bool             _moving;
    private bool             _didPeek;
    private bool             _facingDone;
    private float            _waitElapsed;

    protected override Status OnStart()
    {
        if (_enemy == null)
        {
            _enemy = Agent.Value.GetComponent<EnemyBase>();
            _cover = Agent.Value.GetComponent<EnemyCoverModule>();
        }

        if (_enemy == null || _cover == null) return Status.Failure;
        if (_enemy.State.IsDead)              return Status.Failure;

        _moving      = false;
        _didPeek     = false;
        _facingDone  = false;
        _waitElapsed = 0f;

        if (_enemy.State.PlayerIsSeen) return Status.Success;

        if (!_cover.TryGetPeekPosition(out Vector3 peekPos))
            return Status.Failure;

        _enemy.Navigation.MoveTo(peekPos, run: true);
        _moving = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.IsDead)          return Status.Failure;
        if (!_enemy.State.PlayerDetected) return Status.Failure;

        if (_moving)
        {
            if (_enemy.State.PlayerIsSeen) { _didPeek = true; return Status.Success; }
            if (!_enemy.IsEnemyStopped())  return Status.Running;
            _moving  = false;
            _didPeek = true;
        }

        // Плавно поворачиваемся к игроку пока не встали лицом
        if (!_facingDone)
        {
            _enemy.Navigation.FaceTo(_enemy.PlayerTransform.position);
            _facingDone = _enemy.Navigation.IsFacing(_enemy.PlayerTransform.position);
        }

        if (_enemy.State.PlayerIsSeen) return Status.Success;

        _waitElapsed += Time.deltaTime;
        if (_waitElapsed >= PeekTimeout.Value)
            return Status.Failure;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_didPeek && _cover != null) _cover.IncrementCoverIterations();
        _moving      = false;
        _didPeek     = false;
        _facingDone  = false;
        _waitElapsed = 0f;
    }
}
