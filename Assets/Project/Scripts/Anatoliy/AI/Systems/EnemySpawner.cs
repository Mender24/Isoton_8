using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private bool _findPlayerAutomatically = true;
    
    [Header("Spawn Points")]
    [Tooltip("Если пусто, будут использоваться дочерние объекты спавнера")]
    [SerializeField] private List<Transform> _spawnPoints = new();
    [SerializeField] private bool _useChildrenAsSpawnPoints = true;
    
    [Header("Spawn Intensity")]
    [SerializeField] private bool _enableSpawning = true;
    [SerializeField] private float _spawnInterval = 5f;
    [Tooltip("Случайное отклонение от интервала спавна")]
    [SerializeField] private float _spawnIntervalVariation = 1f;
    [Tooltip("Время, которое бот лежит мертвый после смерти в секундах")]
    [SerializeField] private float _timeAfterDeath = 15f;
    
    [Header("Enemy Count Settings")]
    [SerializeField] private int _maxEnemiesAlive = 5;
    [SerializeField] private bool _endlessMode = true;
    [Tooltip("Максимальное количество врагов для спавна (только если не бесконечный режим)")]
    [SerializeField] private int _totalEnemiesToSpawn = 20;
    
    [Header("AI Behavior Modifications")]
    [Tooltip("Враги сразу идут к игроку при спавне")]
    [SerializeField] private bool _aggroOnSpawn = true;
    [Tooltip("Враги дольше помнят игрока")]
    [SerializeField] private bool _increasedMemory = true;
    [SerializeField] private float _forgetTimeMultiplier = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebug = false;
    [SerializeField] private bool _showSpawnPoints = true;
    [SerializeField] private Color _spawnPointColor = Color.green;
    
    private List<EnemyBase> _activeNewEnemies = new();
    private List<EnemyAI>   _activeEnemies    = new();
    private int _totalSpawnedCount = 0;
    private float _nextSpawnTime = 0f;
    private bool _isSpawning = false;

    public int ActiveEnemyCount => _activeNewEnemies.Count + _activeEnemies.Count;
    public int TotalSpawnedCount => _totalSpawnedCount;
    public bool CanSpawn => _enableSpawning &&
                           (_endlessMode || _totalSpawnedCount < _totalEnemiesToSpawn) &&
                           ActiveEnemyCount < _maxEnemiesAlive;

    #region Initialization
    
    void Start()
    {
        InitializeSpawner();
        
        if (_enableSpawning)
        {
            _nextSpawnTime = Time.time + GetRandomSpawnInterval();
            StartCoroutine(SpawnRoutine());
        }
    }
    
    private void InitializeSpawner()
    {
        if (_findPlayerAutomatically && _playerTransform == null)
        {
            CharacterController player;

            if (SceneLoader.instance != null && SceneLoader.instance.Player != null)
                player = SceneLoader.instance.Player.GetComponent<CharacterController>();
            else
                player = FindFirstObjectByType<CharacterController>();

            if (player != null)
            {
                _playerTransform = player.transform;
                if (_showDebug)
                    Debug.Log($"[EnemySpawner] Player found automatically: {_playerTransform.name}");
            }
            else
            {
                Debug.LogError("[EnemySpawner] Player not found! Assign manually or add CharacterController to player.");
            }
        }
        
        if (_useChildrenAsSpawnPoints && _spawnPoints.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _spawnPoints.Add(transform.GetChild(i));
            }
            
            if (_showDebug)
                Debug.Log($"[EnemySpawner] Found {_spawnPoints.Count} spawn points from children");
        }
        
        if (_spawnPoints.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No spawn points assigned! Using spawner position.");
            _spawnPoints.Add(transform);
        }
        
        if (_enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy prefab not assigned!");
        }
    }
    
    #endregion
    
    #region Spawning
    
    private IEnumerator SpawnRoutine()
    {
        _isSpawning = true;
        
        while (_isSpawning)
        {
            if (Time.time >= _nextSpawnTime && CanSpawn)
            {
                SpawnEnemy();
                _nextSpawnTime = Time.time + GetRandomSpawnInterval();
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public MonoBehaviour SpawnEnemy()
    {
        if (_enemyPrefab == null || _spawnPoints.Count == 0)
        {
            Debug.LogError("[EnemySpawner] Cannot spawn - missing prefab or spawn points!");
            return null;
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject enemyObject = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyBase newEnemy = enemyObject.GetComponent<EnemyBase>();
        if (newEnemy != null)
        {
            SetupNewEnemy(newEnemy);
            _activeNewEnemies.Add(newEnemy);
            _totalSpawnedCount++;

            if (_showDebug)
                Debug.Log($"[EnemySpawner] Spawned new enemy #{_totalSpawnedCount} at {spawnPoint.name}. Active: {ActiveEnemyCount}/{_maxEnemiesAlive}");

            return newEnemy;
        }

        EnemyAI legacyEnemy = enemyObject.GetComponent<EnemyAI>();
        if (legacyEnemy != null)
        {
            SetupLegacyEnemy(legacyEnemy);
            _activeEnemies.Add(legacyEnemy);
            _totalSpawnedCount++;

            if (_showDebug)
                Debug.Log($"[EnemySpawner] Spawned legacy enemy #{_totalSpawnedCount} at {spawnPoint.name}. Active: {ActiveEnemyCount}/{_maxEnemiesAlive}");

            return legacyEnemy;
        }

        Debug.LogError("[EnemySpawner] Spawned prefab has neither EnemyBase nor EnemyAI component!");
        Destroy(enemyObject);
        return null;
    }

    // ── Новая система ──────────────────────────────────────────────────────────

    private void SetupNewEnemy(EnemyBase enemy)
    {
        if (_increasedMemory)
            enemy.Perception.MultiplyForgetTime(_forgetTimeMultiplier);

        enemy.State.IsActivated = true;

        if (_aggroOnSpawn && _playerTransform != null)
            StartCoroutine(AggroNewEnemyOnSpawn(enemy));

        StartCoroutine(MonitorNewEnemyDeath(enemy));
    }

    private IEnumerator AggroNewEnemyOnSpawn(EnemyBase enemy)
    {
        // Ждём Start() врага
        yield return new WaitForSeconds(0.1f);

        if (enemy == null || enemy.State.IsDead) yield break;

        enemy.State.IsAlerted = true;
        enemy.State.PlayerDetected = true;
        enemy.State.LastKnownPlayerPosition = _playerTransform.position;
        enemy.State.TimeSinceLastSeen = 0f;
        enemy.Navigation.SetSpeed(enemy.Navigation.RunSpeed);

        if (_showDebug)
            Debug.Log("[EnemySpawner] New enemy aggro'd on spawn");
    }

    private IEnumerator MonitorNewEnemyDeath(EnemyBase enemy)
    {
        while (enemy != null && !enemy.State.IsDead)
            yield return new WaitForSeconds(0.5f);

        _activeNewEnemies.Remove(enemy);

        if (_showDebug)
            Debug.Log($"[EnemySpawner] New enemy died. Active: {ActiveEnemyCount}/{_maxEnemiesAlive}");

        if (enemy != null)
        {
            yield return new WaitForSeconds(_timeAfterDeath);
            Destroy(enemy.gameObject);
        }
    }

    // ── Legacy система ─────────────────────────────────────────────────────────

    private void SetupLegacyEnemy(EnemyAI enemy)
    {
        enemy.spawnType = EnemyAI.SpawnSource.FromSpawner;
        enemy.playerTransform = _playerTransform;

        if (_increasedMemory)
            enemy.forgetTime *= _forgetTimeMultiplier;

        enemy.isActivated = true;

        if (_aggroOnSpawn && _playerTransform != null)
            StartCoroutine(AggroLegacyEnemyOnSpawn(enemy));

        StartCoroutine(MonitorLegacyEnemyDeath(enemy));
    }

    private IEnumerator AggroLegacyEnemyOnSpawn(EnemyAI enemy)
    {
        yield return new WaitForSeconds(0.1f);

        if (enemy != null && _playerTransform != null)
        {
            enemy.isAlerted = true;
            enemy.playerDetected = true;
            enemy.lastKnownPlayerPosition = _playerTransform.position;
            enemy.startPosition = enemy.transform.position;
            enemy.timeSinceLastSeen = 0f;

            if (enemy.agent != null)
                enemy.agent.speed = enemy.runSpeed;

            if (_showDebug)
                Debug.Log("[EnemySpawner] Legacy enemy aggro'd on spawn");
        }
    }

    private IEnumerator MonitorLegacyEnemyDeath(EnemyAI enemy)
    {
        while (enemy != null && enemy.isActivated)
            yield return new WaitForSeconds(0.5f);

        _activeEnemies.Remove(enemy);

        if (_showDebug)
            Debug.Log($"[EnemySpawner] Legacy enemy died. Active: {ActiveEnemyCount}/{_maxEnemiesAlive}");

        if (enemy != null)
        {
            yield return new WaitForSeconds(_timeAfterDeath);
            Destroy(enemy.gameObject);
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Count)];
    }
    
    private float GetRandomSpawnInterval()
    {
        return _spawnInterval + Random.Range(-_spawnIntervalVariation, _spawnIntervalVariation);
    }
    
    #endregion
    
    #region Public Controls
    
    public void SetSpawningEnabled(bool enabled)
    {
        _enableSpawning = enabled;
        
        if (enabled && !_isSpawning)
        {
            _nextSpawnTime = Time.time + GetRandomSpawnInterval();
            StartCoroutine(SpawnRoutine());
        }
        
        if (_showDebug)
            Debug.Log($"[EnemySpawner] Spawning {(enabled ? "enabled" : "disabled")}");
    }
    
    public MonoBehaviour ForceSpawnEnemy()
    {
        return SpawnEnemy();
    }
    
    public void ForceSpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (CanSpawn)
                SpawnEnemy();
        }
    }
    
    public void KillAllEnemies()
    {
        foreach (var enemy in _activeEnemies.ToArray())
        {
            if (enemy != null)
            {
                enemy.Health = 0;
                enemy.Damage(999, gameObject);
            }
        }
        
        if (_showDebug)
            Debug.Log("[EnemySpawner] Killed all enemies");
    }
    
    public void ResetSpawnCount()
    {
        _totalSpawnedCount = 0;
        
        if (_showDebug)
            Debug.Log("[EnemySpawner] Spawn count reset");
    }
    
    public void SetSpawnInterval(float interval)
    {
        _spawnInterval = Mathf.Max(0.5f, interval);
    }
    
    public void SetMaxEnemies(int max)
    {
        _maxEnemiesAlive = Mathf.Max(1, max);
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmos()
    {
        if (!_showDebug || !_showSpawnPoints) return;
        
        List<Transform> pointsToShow = new List<Transform>(_spawnPoints);
        
        if (_useChildrenAsSpawnPoints && pointsToShow.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                pointsToShow.Add(transform.GetChild(i));
            }
        }
        
        if (pointsToShow.Count == 0)
        {
            pointsToShow.Add(transform);
        }
        
        Gizmos.color = _spawnPointColor;
        
        foreach (var point in pointsToShow)
        {
            if (point == null) continue;
            
            Gizmos.DrawWireSphere(point.position, 0.5f);
            
            Gizmos.DrawRay(point.position, point.forward * 2f);
            
            Gizmos.DrawLine(point.position + Vector3.left * 0.5f, point.position + Vector3.right * 0.5f);
            Gizmos.DrawLine(point.position + Vector3.forward * 0.5f, point.position + Vector3.back * 0.5f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (_showDebug)
        {
            string info = $"Enemy Spawner\n";
            info += $"Active: {_activeEnemies.Count}/{_maxEnemiesAlive}\n";
            info += $"Total Spawned: {_totalSpawnedCount}";
            
            if (!_endlessMode)
                info += $"/{_totalEnemiesToSpawn}";
            
            info += $"\nSpawning: {(_enableSpawning ? "ON" : "OFF")}";
            
            if (_enableSpawning && Application.isPlaying)
            {
                float timeToNext = _nextSpawnTime - Time.time;
                info += $"\nNext spawn in: {Mathf.Max(0, timeToNext):F1}s";
            }
            
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, info);
        }
#endif
    }
    
    #endregion
    
    void OnDestroy()
    {
        _isSpawning = false;
    }
}