using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Управление всеми анимациями врага
/// Полностью интегрирован с EnemyAI
/// </summary>
public class BasicEnemyAnimationController : MonoBehaviour
{
    public enum MeleeAttackType { Stationary = 1, InMotion = 0 }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private EnemyAI _enemyAI;
    
    [Header("Movement Animation Settings")]
    [SerializeField] private float _speedSmoothTime = 0.3f;
    [SerializeField] private float _minSpeedThreshold = 0.2f;

    [Tooltip("Множитель скорости для более выразительных анимаций")]
    [SerializeField] private float _speedMultiplier = 1.0f;
    public MeleeAttackType attackType = MeleeAttackType.Stationary;

    [Header("Idle Settings")]
    [SerializeField] private float _idleVariationInterval = 5f;
    [SerializeField] private int _idleVariationsCount = 3;
    [SerializeField] private float _idleVariationChance = 0.6f; // 60% шанс вариации
    [SerializeField] private int _amountWinAnimations = 3;
    
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

        //Melee
        public const string MeleeAttack = "MeleeAttack";
        public const string MeleeAttacking = "MeleeAttacking";
        public const string MeleeSpeed = "MeleeSpeed";
        public const string PlayerDetected = "PlayerDetected";
        public const string HasAlerted = "HasAlerted";
        public const string MeleeAttackType = "MeleeAttackType";
        
        // States
        public const string IsDead = "IsDead";
        public const string RandomIdleF = "RandomIdleF";
        public const string WinNumber = "WinNumber";
        
        // Triggers
        public const string Alert = "Alert";
        public const string Shoot = "Shoot";
        public const string Hit = "Hit";
        public const string Reload = "Reload";
        public const string Search = "Search";
        public const string Winning = "Win";
    }
    
    // State cache для оптимизации
    private bool _cachedWalking = false;
    private bool _cachedRunning = false;
    private bool _cachedAiming = false;
    private bool _cachedAlerted = false;
    private bool _cachedPlayerDetected = false;
    private bool _cachedReloading = false;
    private bool _cachedMeleeAttacking = false;
    private float _cachedSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _idleFloat = 0.0f;
    private float _randomIdle = 0.0f;
    
    private float _idleTimer = 0f;
    private float _stationaryTime = 0f;
    private bool _wasMoving = false;
    private float _reloadClipLength = 2.8f;
    private float _meleeAttackClipLength = 3.2f;
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
        if (_enemyAI.combatType == EnemyAI.CombatType.Ranged)
        {
            var animController = GetComponent<Animator>().runtimeAnimatorController;
            var clip = animController.animationClips.First(a => a.name == "ReloadAssaultRifle");
            _reloadClipLength = clip.length;
        }
        else if (_enemyAI.combatType == EnemyAI.CombatType.Melee)
        {
            var animController = GetComponent<Animator>().runtimeAnimatorController;
            var clip = animController.animationClips.First(a => a.name == "AttackRightClaws1Creature_RM");
            _meleeAttackClipLength = clip.length;
        }
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
        // float normalizedSpeed = _agent.speed > 0 ? Mathf.Clamp01(currentSpeed / _agent.speed) : 0f;

        float maxSpeed = Mathf.Max(_agent.speed, 0.001f);
        _targetSpeed = Mathf.Clamp01(currentSpeed / maxSpeed) * _speedMultiplier;

        _targetSpeed = Mathf.Clamp01(currentSpeed / maxSpeed) * _speedMultiplier;
        
        if (_targetSpeed < _minSpeedThreshold)
        {
            _targetSpeed = 0f;
        }
        
        _cachedSpeed = Mathf.Lerp(_cachedSpeed, _targetSpeed, _speedSmoothTime * Time.deltaTime * 10f);
        _animator.SetFloat(AnimParams.Speed, _cachedSpeed);
        
        bool isMoving = currentSpeed > _minSpeedThreshold;
        
        if (isMoving)
        {
            _wasMoving = true;
            _stationaryTime = 0f;
            attackType = MeleeAttackType.InMotion;
        }
        else if (_wasMoving)
        {
            _wasMoving = false;
            _stationaryTime = 0f;
        }
        else
        {
            _stationaryTime += Time.deltaTime;
            attackType = MeleeAttackType.Stationary;
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
        
        if (_enemyAI.playerDetected != _cachedPlayerDetected)
        {
            _cachedPlayerDetected = _enemyAI.playerDetected;
            _animator.SetBool(AnimParams.PlayerDetected, _cachedPlayerDetected);

            if (_showDebugLogs)
                Debug.Log($"[Animation] PlayerDetected: {_cachedPlayerDetected}");
        }

        if (_enemyAI.isReload != _cachedReloading)
        {
            _cachedReloading = _enemyAI.isReload;
            
            // Делаем длину перезарядки по параметку в EnemyAI
            float reloadSpeed = _reloadClipLength / _enemyAI.timeReload;
            _animator.SetFloat(AnimParams.ReloadSpeed, reloadSpeed);

            _animator.SetBool(AnimParams.Reloading, _cachedReloading);
            
            if (_showDebugLogs)
                Debug.Log($"[Animation] Reloading: {_cachedReloading}");
        }

        if (_enemyAI.isMeleeAttacking != _cachedMeleeAttacking)
        {
            _cachedMeleeAttacking = _enemyAI.isMeleeAttacking;

            // Устанавливаем как будем атаковать, в движении или нет
            SetMeleeAttackType();

            // Делаем длину удара по параметку в EnemyAI
            float meleeSpeed = _meleeAttackClipLength / _enemyAI.meleeAttackLength;
            _animator.SetFloat(AnimParams.MeleeSpeed, meleeSpeed);

            _animator.SetBool(AnimParams.MeleeAttacking, _enemyAI.isMeleeAttacking);
            if (_showDebugLogs)
                Debug.Log($"[Animation] Is MeleeAttacking: {_cachedMeleeAttacking}");
        }
        
        _animator.SetBool(AnimParams.Shooting, _enemyAI.isFire);
    }
    
    #endregion
    
    #region Idle Variations
    
    private void UpdateIdleVariations()
    {
        // Условие, при котором бот может играть вариации ожидания
        bool canPlayVariation = !_enemyAI.isAlerted && 
                                !_agent.hasPath && 
                                _agent.velocity.magnitude < 0.1f &&
                                !_enemyAI.playerDetected;
        
        if (canPlayVariation)
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
            _idleTimer = 0f;
            _randomIdle = 0f;
        }

        _idleFloat = Mathf.MoveTowards(_idleFloat, _randomIdle, Time.deltaTime * 0.5f);
        
        _animator.SetFloat(AnimParams.RandomIdleF, _idleFloat);
    }
    
    private void PlayRandomIdleVariation()
    {
        float roll = Random.value;

        if (roll > _idleVariationChance)
        {
            _randomIdle = 0f;
            return;
        }
        
        int nextVariation;
        do
        {
            nextVariation = Random.Range(1, _idleVariationsCount + 1);
        } 
        while (nextVariation == (int)_randomIdle && _idleVariationsCount > 1);

        _randomIdle = (float)nextVariation;
        
        if (_showDebugLogs)
            Debug.Log($"[Animation] New Idle target variation: {_randomIdle}");
    }
    
    public void SetMeleeAttackType()
    {
        _animator.SetFloat(AnimParams.MeleeAttackType, (float)attackType);
    }

    #endregion
    
    #region Public Animation Controls
    
    /// <summary>
    /// Обнуляет все состояния возвращая в изначальное
    /// </summary>
    public void ResetAnimationController()
    {
        _animator.SetBool(AnimParams.IsAlerted, false);
        _animator.SetBool(AnimParams.Walking, false);
        _animator.SetBool(AnimParams.Running, false);
        _animator.SetBool(AnimParams.Aiming, false);
        _animator.SetBool(AnimParams.Shooting, false);
        _animator.SetBool(AnimParams.Reloading, false);
        _animator.SetBool(AnimParams.MeleeAttacking, false);

        ResetAllCachedValues();
        ResetAllTriggers();
    }

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
    /// Проигрывает анимацию ближней атаки
    /// </summary>
    public void PlayMeleeAttack()
    {
        if (!_isInitialized) return;
        
        // Делаем длину удара по параметку в EnemyAI
        float meleeSpeed = _meleeAttackClipLength / _enemyAI.meleeAttackLength;
        _animator.SetFloat(AnimParams.MeleeSpeed, meleeSpeed);
        
        _animator.SetTrigger(AnimParams.MeleeAttack);

        if (_showDebugLogs)
            Debug.Log("[Animation] Melee attack triggered!");
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
    /// Проигрывает случайную анимацию победы
    /// </summary>
    public void PlayWinning()
    {
        if (!_isInitialized) return;

        int roll = Random.Range(0, _amountWinAnimations);

        _animator.SetTrigger(AnimParams.Winning);
        _animator.SetInteger(AnimParams.WinNumber, roll);
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

    /// <summary>
    /// Принудительно задать конкретную idle вариацию
    /// </summary>
    public void SetIdleVariation(int variationIndex)
    {
        if (!_isInitialized) return;
        
        _animator.SetFloat(AnimParams.RandomIdleF, variationIndex);
        
        if (_showDebugLogs)
            Debug.Log($"[Animation] Set idle variation to: {variationIndex}");
    }

    /// <summary>
    /// Получить текущую скорость анимации
    /// </summary>
    public float GetAnimationSpeed()
    {
        return _cachedSpeed;
    }
    
    #endregion
    
    #region Animation Events (вызываются из анимаций)
    
    public void OnAlertStart()
    {
        _enemyAI.AlertStarted();
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Alert start");
    }

    public void OnAlertComplete()
    {
        SetAiming(true);
        _animator.SetBool(AnimParams.HasAlerted, true);
        _enemyAI.AlertCompleted();
        
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
    
    public void OnHitReactionStart()
    {
        _enemyAI.agent.isStopped = true;
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Hit reaction start");
    }

    public void OnHitReactionComplete()
    {
        _enemyAI.agent.isStopped = false;
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Hit reaction complete");
    }
    
    public void OnDeathAnimationStart()
    {
        // _enemyAI.agent.isStopped = true;
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Death animation complete");
    }

    public void OnDeathAnimationComplete()
    {
        // _enemyAI.agent.isStopped = false;
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

    public void OnMeleeAttackHit()
    {
        _enemyAI?.ExecuteMeleeAttack();
        
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Melee attack hit!");
    }

    public void OnMeleeAttackComplete()
    {
        if (_showDebugLogs)
            Debug.Log("[Animation Event] Melee attack complete!");
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
        _animator.ResetTrigger(AnimParams.Winning);
    }

    /// <summary>
    /// Сбросить все кэшируемые значения
    /// </summary>
    public void ResetAllCachedValues()
    {
        _cachedAiming = false;
        _cachedAlerted = false;
        _cachedMeleeAttacking = false;
        _cachedPlayerDetected = false;
        _cachedReloading = false;
        _cachedRunning = false;
        _cachedSpeed = 0.0f;
        _cachedWalking = false;
    }

    /// <summary>
    /// Получить текущее состояние движения (для debug)
    /// </summary>
    public string GetMovementState()
    {
        if (_cachedSpeed < 0.05f) return "Idle";
        if (_cachedSpeed < 0.5f) return "Walking";
        return "Running";
    }
    
    #endregion
}