using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerDetected", story: "[Agent] on player detected",
    category: "Action", id: "8725f4fdd3f7bb34cbcd9a1ceb49fff8")]
public partial class PlayerDetectedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private EnemyBase _enemy = null;

    protected override Status OnStart()
    { 
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();
        return Status.Running; 
    }

    protected override Status OnUpdate()
    {
        if (!_enemy) return Status.Failure;
        return _enemy.OnPlayerDetected();
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryToHear", story: "[Agent] checks if hears anything",
    category: "Action", id: "8035a35a8d131d5125a63c856e0f5d9f")]
public partial class TryToHearAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!_enemy) return Status.Failure;
        return _enemy.OnNoiseDetected();
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "OnPlayerDetectedTrigger", story: "[Agent] plays detection animation",
    category: "Action", id: "ff484dcfaa9e70c54778b7755ba6816a")]
public partial class OnPlayerDetectedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy != null)
            _enemy.Animator?.PlayAlert();
        else
            return Status.Failure;

        return Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToNoise", story: "[Agent] moves to the heard noise",
    category: "Action", id: "9e7be751201f2bdd0fbec0f801b19bf1")]
public partial class MoveToNoiseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null) return Status.Failure;

        Vector3 target = _enemy.GetNoiseInvestigationTarget();

        // Move to navmesh on y axis
        if (Physics.Raycast(target + Vector3.up, Vector3.down, out RaycastHit hit, 100f))
            target.y = hit.point.y;

        _enemy.Navigation.MoveTo(target, false);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!_enemy.State.HeardNoise || _enemy.State.PlayerDetected)
            return Status.Failure;

        return _enemy.IsEnemyStopped() ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        _enemy?.Navigation.ResetPath();
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToLastKnownPosition", story: "[Agent] moves to last known position",
    category: "Action", id: "ff484dcfaa9e11c54778b5055ba6816a")]
public partial class MoveToLastKnownPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null || _enemy.State.IsDead) return Status.Failure;

        var e = _enemy;
        Vector3 dest = e.State.LastKnownPlayerPosition;
        dest.y = e.transform.position.y;

        e.Navigation.ResetPath();
        e.Navigation.MoveTo(dest, run: true);
        e.State.IsSearching = true;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;

        if (e.State.IsReloading) e.Navigation.SetSpeed(0f);
        else                     e.Navigation.SetSpeed(e.Navigation.RunSpeed);

        return e.IsEnemyStopped() ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (_enemy != null)
            _enemy.State.IsSearching = false;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReturnToStartPoint", story: "[Agent] returns to start point if it exists",
    category: "Action", id: "d1381cb7318f366f389aa132e65d2093")]
public partial class ReturnToStartPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private EnemyBase _enemy = null;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e.State.StartPosition != Vector3.zero && !e.State.IsDead)
            e.Navigation.MoveTo(e.State.StartPosition, run: false);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy.State.PlayerDetected) return Status.Failure;
        return _enemy.IsEnemyStopped() ? Status.Success : Status.Running;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToRandomPoint", story: "[Agent] moves to random point",
    category: "Action", id: "7e8e51831ef6fe53fb091644598f85a0")]
public partial class MoveToRandomPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(5f);
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new(3f);
    private EnemyBase _enemy = null;

    private float _cachedStoppingDistance;
    private int   _attempts;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e == null || e.State.IsDead) return Status.Failure;

        _attempts = 0;
        _cachedStoppingDistance = e.Navigation.Agent.stoppingDistance;
        e.Navigation.SetStoppingDistance(StoppingDistance.Value);
        e.State.IsSearching = true;
        e.Animator?.PlaySearch();

        TryMoveToRandom();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;

        if (e.State.PlayerDetected || e.State.IsFiring) return FinishSearch(e, Status.Success);

        if (e.IsEnemyStopped())
        {
            _attempts++;
            if (_attempts >= 1) return FinishSearch(e, Status.Success);
            TryMoveToRandom();
        }

        return Status.Running;
    }

    private Status FinishSearch(EnemyBase e, Status result)
    {
        e.Navigation.ResetPath();
        e.State.IsSearching = false;
        e.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        return result;
    }

    private void TryMoveToRandom()
    {
        if (_enemy.Navigation.TryGetRandomNavPoint(
            _enemy.transform.position, SearchRadius.Value, out Vector3 point))
        {
            _enemy.Navigation.MoveTo(point, run: false);
        }
    }

    protected override void OnEnd()
    {
        if (_enemy != null)
        {
            _enemy.State.IsSearching = false;
            _enemy.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        }
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SearchPlayer", story: "[Agent] searches player",
    category: "Action", id: "ff484dcfaa9e70c54778b6355ba6816a")]
public partial class SearchPatternNode : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SearchRadius     = new(5f);
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new(3f);
    private EnemyBase _enemy = null;

    private float _cachedStoppingDistance;
    private int   _searchAttempts;
    private const int _maxAttempts = 3;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        var e = _enemy;
        if (e == null || e.State.IsDead) return Status.Failure;

        _searchAttempts = 0;
        _cachedStoppingDistance = e.Navigation.Agent.stoppingDistance;
        e.Navigation.SetStoppingDistance(StoppingDistance.Value);
        e.State.IsSearching = true;
        e.Animator?.PlaySearch();

        if (!e.Navigation.Agent.hasPath)
            TryMoveToSearchPoint(e);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;

        if (e.State.PlayerDetected)
        {
            return Finish(e, Status.Failure);
        }

        if (e.IsEnemyStopped())
        {
            _searchAttempts++;
            if (_searchAttempts >= _maxAttempts) return Finish(e, Status.Success);
            TryMoveToSearchPoint(e);
        }

        return Status.Running;
    }

    private Status Finish(EnemyBase e, Status result)
    {
        e.Navigation.ResetPath();
        e.State.IsSearching = false;
        e.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        return result;
    }

    private void TryMoveToSearchPoint(EnemyBase e)
    {
        if (e.Navigation.TryGetRandomNavPoint(
            e.State.LastKnownPlayerPosition, SearchRadius.Value, out Vector3 point))
        {
            e.Navigation.MoveTo(point);
        }
    }

    protected override void OnEnd()
    {
        if (_enemy != null)
        {
            _enemy.State.IsSearching = false;
            _enemy.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        }
    }
}