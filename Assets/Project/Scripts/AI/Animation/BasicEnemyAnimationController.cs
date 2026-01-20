using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Управление всеми анимациями врага
/// Полностью интегрирован с EnemyAI
/// </summary>
public class BasicEnemyAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private EnemyAI _enemyAI;
    
    [Header("Movement Animation Settings")]
    [SerializeField] private float _walkSpeedThreshold = 0.3f;
    [SerializeField] private float _runSpeedThreshold = 0.7f;
    [SerializeField] private float _animationSmoothTime = 0.1f;
    
    [Header("Idle Settings")]
    [SerializeField] private float _idleVariationInterval = 8f;
    [SerializeField] private int _idleVariationsCount = 3;
    [SerializeField] private float _idleVariationChance = 0.6f; // 60% шанс вариации
    
    [Header("Combat Animation Settings")]
    [SerializeField] private float _aimRotationSpeed = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;
    
    private static class AnimParams
    {
        // Movement
        public const string Speed = "Speed";
        public const string Walking = "Walking";
        public const string Running = "Running";
        
        // Combat
        public const string Aiming = "Aiming";
        public const string IsAlerted = "IsAlerted";
        public const string Shooting = "Shooting";
        public const string Reloading = "Reloading";
        public const string ReloadSpeed = "ReloadSpeed";
        
        // States
        public const string IsDead = "IsDead";
        public const string RandomIdle = "RandomIdle";
        
        // Triggers
        public const string Alert = "Alert";
        public const string Shoot = "Shoot";
        public const string Hit = "Hit";
        public const string Reload = "Reload";
        public const string Search = "Search";
    }
    
    // State cache для оптимизации
    private bool _cachedWalking = false;
    private bool _cachedRunning = false;
    private bool _cachedAiming = false;
    private bool _cachedAlerted = false;
    private bool _cachedReloading = false;
    private float _cachedSpeed = 0f;
    
    // Idle variation
    private float _idleTimer = 0f;
    private float reloadClipLength = 2.8f;
    private bool _isInitialized = false;

    
    #region Initialization
    
    void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
        
        if (_enemyAI == null)
            _enemyAI = GetComponent<EnemyAI>();
        
        if (_animator == null)
        {
            Debug.LogError($"[EnemyAnimationController] Animator not found on {gameObject.name}!");
            return;
        }

        _isInitialized = true;
        
        if (_showDebugLogs)
            Debug.Log($"[EnemyAnimationController] Initialized on {gameObject.name}");
    }

    void Start()
    {
        var animController = GetComponent<Animator>().runtimeAnimatorController;
        var clip = animController.animationClips.First(a => a.name == "Reloading");
        reloadClipLength = clip.length;
    }

    void OnValidate()
    {
        // Автоматически находим компоненты в редакторе
        if (_animator == null)
            _animator = GetComponent<Animator>();
        
        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
        
        if (_enemyAI == null)
            _enemyAI = GetComponent<EnemyAI>();

    }
    
    #endregion
    
    #region Update Loop
    
    void Update()
    {
        if (!_isInitialized || _animator == null || _enemyAI == null) return;
        
        UpdateMovementAnimations();
        UpdateCombatAnimations();
        UpdateIdleVariations();
    }
    
    #endregion
    
    #region Movement Animations
    
    private void UpdateMovementAnimations()
    {
        if (_agent == null) return;
        
        float currentSpeed = _agent.velocity.magnitude;
        float normalizedSpeed = _agent.speed > 0 ? Mathf.Clamp01(currentSpeed / _agent.speed) : 0f;
        
        if (Mathf.Abs(normalizedSpeed - _cachedSpeed) > 0.01f)
        {
            _cachedSpeed = Mathf.Lerp(_cachedSpeed, normalizedSpeed, _animationSmoothTime);
            _animator.SetFloat(AnimParams.Speed, _cachedSpeed);
        }
        
        bool isWalking = currentSpeed > 0.1f && normalizedSpeed < _runSpeedThreshold;
        bool isRunning = normalizedSpeed >= _runSpeedThreshold;
        
        if (isWalking != _cachedWalking)
        {
            _cachedWalking = isWalking;
            _animator.SetBool(AnimParams.Walking, isWalking);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Walking: {isWalking}");
        }
        
        if (isRunning != _cachedRunning)
        {
            _cachedRunning = isRunning;
            _animator.SetBool(AnimParams.Running, isRunning);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Running: {isRunning}");
        }
    }
    
    #endregion
    
    #region Combat Animations
    
    private void UpdateCombatAnimations()
    {
        if (_enemyAI.isAlerted != _cachedAlerted)
        {
            _cachedAlerted = _enemyAI.isAlerted;
            _animator.SetBool(AnimParams.IsAlerted, _cachedAlerted);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Alerted: {_cachedAlerted}");
        }
        
        if (_enemyAI.isReload != _cachedReloading)
        {
            _cachedReloading = _enemyAI.isReload;

            // Делаем длину перезарядке по параметку в EnemyAI
            float reloadSpeed = reloadClipLength / _enemyAI.timeReload;
            _animator.SetFloat(AnimParams.ReloadSpeed, reloadSpeed);

            _animator.SetBool(AnimParams.Reloading, _cachedReloading);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Reloading: {_cachedReloading}");
        }
        
        // Обновляем состояние стрельбы
        _animator.SetBool(AnimParams.Shooting, _enemyAI.isFire);
    }
    
    #endregion
    
    #region Idle Variations
    
    private void UpdateIdleVariations()
    {
        // Вариации idle только когда враг не в тревоге и не двигается
        bool shouldPlayVariation = !_enemyAI.isAlerted && 
                                   !_agent.hasPath && 
                                   _agent.velocity.magnitude < 0.1f &&
                                   !_enemyAI.playerDetected;
        
        if (shouldPlayVariation)
        {
            _idleTimer += Time.deltaTime;
            
            if (_idleTimer >= _idleVariationInterval)
            {
                PlayRandomIdleVariation();
                _idleTimer = 0f;
            }
        }
        else
        {
            // Сбрасываем таймер и возвращаем базовую idle
            _idleTimer = 0f;
            
            if (_animator.GetInteger(AnimParams.RandomIdle) != 0)
            {
                _animator.SetInteger(AnimParams.RandomIdle, 0);
            }
        }
    }
    
    private void PlayRandomIdleVariation()
    {
        // Случайный выбор с учетом шанса
        if (Random.value > _idleVariationChance)
        {
            _animator.SetInteger(AnimParams.RandomIdle, 0);
            return;
        }
        
        int randomIdle = Random.Range(1, _idleVariationsCount + 1);
        _animator.SetInteger(AnimParams.RandomIdle, randomIdle);
        
        if (_showDebugLogs)
            Debug.Log($"[Animation] Playing Idle variation: {randomIdle}");
    }
    
    #endregion
    
    #region Public Animation Controls
    
    /// <summary>
    /// Проигрывает анимацию обнаружения игрока
    /// </summary>
    public void PlayAlert()
    {
        if (!_isInitialized) return;
        
        _animator.SetTrigger(AnimParams.Alert);
        _animator.SetBool(AnimParams.IsAlerted, true);
        
        if (_showDebugLogs)
            Debug.Log("[Animation] Alert triggered!");
    }
    
    /// <summary>
    /// Включает/выключает анимацию прицеливания
    /// </summary>
    public void SetAiming(bool aiming)
    {
        if (!_isInitialized) return;
        
        if (aiming != _cachedAiming)
        {
            _cachedAiming = aiming;
            _animator.SetBool(AnimParams.Aiming, aiming);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Aiming: {aiming}");
        }
    }
    
    /// <summary>
    /// Проигрывает анимацию выстрела
    /// </summary>
    public void PlayShoot()
    {
        if (!_isInitialized) return;
        
        _animator.SetTrigger(AnimParams.Shoot);
        
        if (_showDebugLogs)
            Debug.Log("[Animation] Shoot triggered!");
    }
    
    /// <summary>
    /// Проигрывает анимацию получения урона
    /// </summary>
    public void PlayHit()
    {
        if (!_isInitialized) return;
        
        _animator.SetTrigger(AnimParams.Hit);
        
        if (_showDebugLogs)
            Debug.Log("[Animation] Hit triggered!");
    }
    
    /// <summary>
    /// Проигрывает анимацию перезарядки
    /// </summary>
    public void PlayReload()
    {
        if (!_isInitialized) return;
        
        _animator.SetTrigger(AnimParams.Reload);
        _animator.SetBool(AnimParams.Reloading, true);
        
        if (_showDebugLogs)
            Debug.Log("[Animation] Reload triggered!");
    }
    
    /// <summary>
    /// Проигрывает анимацию поиска игрока
    /// </summary>
    public void PlaySearch()
    {
        if (!_isInitialized) return;
        
        _animator.SetTrigger(AnimParams.Search);
        
        if (_showDebugLogs)
            Debug.Log("[Animation] Search triggered!");
    }
    
    /// <summary>
    /// Устанавливает состояние смерти
    /// </summary>
    public void SetDead(bool isDead)
    {
        if (!_isInitialized) return;
        
        _animator.SetBool(AnimParams.IsDead, isDead);
        
        if (_showDebugLogs)
            Debug.Log($"[Animation] Dead: {isDead}");
    }
    
    /// <summary>
    /// Принудительно устанавливает Walking
    /// </summary>
    public void SetWalking(bool walking)
    {
        if (!_isInitialized) return;
        
        _cachedWalking = walking;
        _animator.SetBool(AnimParams.Walking, walking);
    }
    
    /// <summary>
    /// Принудительно устанавливает Running
    /// </summary>
    public void SetRunning(bool running)
    {
        if (!_isInitialized) return;
        
        _cachedRunning = running;
        _animator.SetBool(AnimParams.Running, running);
    }
    
    #endregion
    
    #region Animation Events (вызываются из анимаций)
    
    public void OnAlertComplete()
    {
        SetAiming(true);
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Alert complete - switching to aim");
    }
    
    public void OnShootStart()
    {
        // Момент выстрела - вызывается в момент вспышки
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Shoot started");
    }
    
    public void OnShootComplete()
    {
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Shoot complete");
    }
    
    public void OnReloadStart()
    {
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Reload started");
    }
    
    public void OnReloadComplete()
    {
        _animator.SetBool(AnimParams.Reloading, false);
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Reload complete");
    }
    
    public void OnHitReactionComplete()
    {
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Hit reaction complete");
    }
    
    public void OnDeathAnimationComplete()
    {
        _enemyAI?.OnDeathComplete();
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Death animation complete");
    }
    
    public void OnSearchComplete()
    {
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Search animation complete");
    }
    
    // Footstep sounds
    public void OnFootstepLeft()
    {
        _enemyAI?.PlayFootstepSound(0);
    }
    
    public void OnFootstepRight()
    {
        _enemyAI?.PlayFootstepSound(1);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Получить текущую скорость анимации
    /// </summary>
    public float GetCurrentSpeed()
    {
        return _animator != null ? _animator.GetFloat(AnimParams.Speed) : 0f;
    }
    
    /// <summary>
    /// Проверить, проигрывается ли определенная анимация
    /// </summary>
    public bool IsPlayingAnimation(string stateName, int layer = 0)
    {
        if (_animator == null) return false;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName(stateName);
    }
    
    /// <summary>
    /// Получить прогресс текущей анимации (0-1)
    /// </summary>
    public float GetAnimationProgress(int layer = 0)
    {
        if (_animator == null) return 0f;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime % 1f;
    }
    
    /// <summary>
    /// Сбросить все триггеры
    /// </summary>
    public void ResetAllTriggers()
    {
        if (_animator == null) return;
        
        _animator.ResetTrigger(AnimParams.Alert);
        _animator.ResetTrigger(AnimParams.Shoot);
        _animator.ResetTrigger(AnimParams.Hit);
        _animator.ResetTrigger(AnimParams.Reload);
        _animator.ResetTrigger(AnimParams.Search);
    }
    
    #endregion
}