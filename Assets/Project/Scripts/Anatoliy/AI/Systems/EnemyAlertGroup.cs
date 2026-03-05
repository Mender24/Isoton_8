using System.Collections.Generic;
using UnityEngine;

public class EnemyAlertGroup : MonoBehaviour
{
    [Header("Enemies in Group")]
    [Tooltip("Оставьте пустым для автосбора из дочерних объектов")]
    [SerializeField] private List<EnemyBase> _enemies = new();

    [Header("Alert Settings")]
    [Tooltip("Дополнительно оповещать врагов в радиусе (не только из списка)")]
    [SerializeField] private bool _useAlertRadius = false;
    [SerializeField] private float _alertRadius = 20f;
    [SerializeField] private LayerMask _enemyLayer = ~0;

    [Header("Debug")]
    [SerializeField] private bool _showDebug = false;

    private bool _isAlerting = false;

    private void Awake()
    {
        if (_enemies.Count == 0)
            _enemies.AddRange(GetComponentsInChildren<EnemyBase>());
    }

    private void Start()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;
            var captured = enemy;
            captured.State.OnAlertedChanged += isAlerted =>
            {
                if (isAlerted)
                    OnEnemyAlerted(captured);
            };
        }
    }

    private void OnEnemyAlerted(EnemyBase source)
    {
        if (_isAlerting) return;
        _isAlerting = true;

        Vector3 playerPos = source.PlayerTransform != null
            ? source.PlayerTransform.position
            : source.State.LastKnownPlayerPosition;

        if (_showDebug)
            Debug.Log($"[EnemyAlertGroup] {source.name} обнаружил игрока, оповещаю группу");

        foreach (var enemy in _enemies)
        {
            if (enemy == null || enemy == source) continue;
            TryAlertEnemy(enemy, playerPos);
        }

        if (_useAlertRadius)
        {
            var colliders = Physics.OverlapSphere(source.transform.position, _alertRadius, _enemyLayer);
            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<EnemyBase>();
                if (enemy == null || enemy == source || _enemies.Contains(enemy)) continue;
                TryAlertEnemy(enemy, playerPos);
            }
        }

        _isAlerting = false;
    }

    private void TryAlertEnemy(EnemyBase enemy, Vector3 playerPos)
    {
        if (!enemy.State.IsActivated || enemy.State.IsDead || enemy.State.IsAlerted) return;

        if (_showDebug)
            Debug.Log($"[EnemyAlertGroup] Оповещён: {enemy.name}");

        enemy.AlertByGroup(playerPos);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_useAlertRadius || !_showDebug) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, _alertRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, _alertRadius);
    }
}
