using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SmartSearchPlayer",
    story: "[Agent] searches player using visible positions",
    category: "Action", id: "a3f92c1d8e4b7f2a9c6d1e5b8a3f92c1")]
public partial class SmartSearchPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(8f);
    [SerializeReference] public BlackboardVariable<int> CandidateCount = new(12);
    [SerializeReference] public BlackboardVariable<int> MaxSearchPoints = new(4);

    private EnemyBase _enemy;
    private int _visitedCount;
    private float _cachedStoppingDistance;
    private LayerMask _obstacleLayer;
    private bool _hasObstacleLayer;

    protected override Status OnStart()
    {
        if (_enemy == null)
            _enemy = Agent.Value.GetComponent<EnemyBase>();

        if (_enemy == null || _enemy.State.IsDead) return Status.Failure;

        _visitedCount = 0;
        _cachedStoppingDistance = _enemy.Navigation.Agent.stoppingDistance;
        _enemy.Navigation.SetStoppingDistance(1.5f);
        _enemy.State.IsSearching = true;
        _enemy.Animator?.PlaySearch();

        if (!_hasObstacleLayer)
        {
            _obstacleLayer = LayerMask.GetMask("Default", "Wall", "Obstacle", "Environment");
            _hasObstacleLayer = true;
        }

        // Нет ни одной достижимой точки — бот не может искать (напр. игрок за пределами NavMesh)
        Vector3 origin = _enemy.State.LastKnownPlayerPosition;
        List<Vector3> initial = GenerateCandidates(origin, SearchRadius.Value, CandidateCount.Value);
        if (initial.Count == 0 &&
            !_enemy.Navigation.TryGetRandomNavPoint(origin, SearchRadius.Value, out _))
        {
            _enemy.Navigation.SetStoppingDistance(_cachedStoppingDistance);
            return Status.Failure;
        }

        MoveToNextSearchPoint();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var e = _enemy;

        if (e.State.PlayerIsSeen) return Finish(Status.Failure);

        if (!e.Navigation.Agent.hasPath || e.IsEnemyStopped())
        {
            _visitedCount++;

            if (_visitedCount >= MaxSearchPoints.Value)
                return Finish(Status.Success);

            MoveToNextSearchPoint();
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_enemy == null) return;
        _enemy.State.IsSearching = false;
        _enemy.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        _enemy.Navigation.ResetPath();
    }

    private Status Finish(Status result)
    {
        _enemy.State.IsSearching = false;
        _enemy.Navigation.SetStoppingDistance(_cachedStoppingDistance);
        _enemy.Navigation.ResetPath();
        return result;
    }

    private void MoveToNextSearchPoint()
    {
        Vector3 target = FindBestSearchPoint();
        _enemy.Navigation.MoveTo(target);
    }

    private Vector3 FindBestSearchPoint()
    {
        Vector3 origin = _enemy.State.LastKnownPlayerPosition;
        float radius = SearchRadius.Value;
        int count = CandidateCount.Value;

        List<Vector3> candidates = GenerateCandidates(origin, radius, count);
        List<Vector3> visible = FilterByVisibility(candidates, origin);

        List<Vector3> pool = visible.Count > 0 ? visible : candidates;

        if (pool.Count == 0)
            return FallbackPoint(origin, radius);

        pool.Sort((a, b) =>
            Vector3.Distance(_enemy.transform.position, a)
                .CompareTo(Vector3.Distance(_enemy.transform.position, b)));

        return pool[0];
    }

    private List<Vector3> GenerateCandidates(Vector3 origin, float radius, int count)
    {
        var result = new List<Vector3>(count);
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            float r = radius * Mathf.Lerp(0.4f, 1f, (float)i / count);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 candidate = origin + dir * r;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (_enemy.Navigation.Agent.CalculatePath(hit.position, path))
                    result.Add(hit.position);
            }
        }

        return result;
    }

    private List<Vector3> FilterByVisibility(List<Vector3> candidates, Vector3 lookTarget)
    {
        var result = new List<Vector3>();
        Vector3 targetEye = lookTarget + Vector3.up * 1.5f;

        foreach (var pos in candidates)
        {
            Vector3 eye = pos + Vector3.up * 1.5f;
            Vector3 dir = (targetEye - eye).normalized;
            float dist = Vector3.Distance(eye, targetEye);

            if (!Physics.Raycast(eye, dir, dist, _obstacleLayer))
                result.Add(pos);
        }

        return result;
    }

    private Vector3 FallbackPoint(Vector3 origin, float radius)
    {
        if (_enemy.Navigation.TryGetRandomNavPoint(origin, radius, out Vector3 point))
            return point;

        return _enemy.transform.position;
    }
}