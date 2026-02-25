using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BasicEnemyAnimator : MonoBehaviour, IEnemyAnimator
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private EnemyState _state;
    [SerializeField] private EnemyNavigation _navigation;

    [Header("Movement")]
    [SerializeField] private float _speedSmoothTime = 0.3f;
    [SerializeField] private float _minSpeedThreshold = 0.2f;
    [SerializeField] private float _speedMultiplier = 1f;

    [Header("Idle variations")]
    [SerializeField] private float _idleVariationInterval = 5f;
    [SerializeField] private int   _idleVariationsCount   = 3;
    [SerializeField] private float _idleVariationChance   = 0.6f;

    [Header("Win animations")]
    [SerializeField] private int _amountWinAnimations = 3;

    [Header("Clip names (for speed sync)")]
    [SerializeField] private string _reloadClipName    = "ReloadAssaultRifle";
    [SerializeField] private string _meleeClipName     = "AttackRightClaws1Creature_RM";

    private static class P
    {
        public const string Speed          = "Speed";
        public const string Aiming         = "Aiming";
        public const string IsAlerted      = "IsAlerted";
        public const string Shooting       = "Shooting";
        public const string Reloading      = "Reloading";
        public const string ReloadSpeed    = "ReloadSpeed";
        public const string MeleeAttack    = "MeleeAttack";
        public const string MeleeAttacking = "MeleeAttacking";
        public const string MeleeSpeed     = "MeleeSpeed";
        public const string MeleeAttackType= "MeleeAttackType";
        public const string HasAlerted     = "HasAlerted";
        public const string IsDead         = "IsDead";
        public const string RandomIdleF    = "RandomIdleF";
        public const string WinNumber      = "WinNumber";
        public const string Alert          = "Alert";
        public const string Hit            = "Hit";
        public const string Reload         = "Reload";
        public const string Search         = "Search";
        public const string Winning        = "Win";
    }

    private float _cachedSpeed;
    private float _idleTimer;
    private float _reloadClipLength = 2.8f;
    private float _meleeClipLength  = 3.2f;
    private bool  _wasMoving;
    private bool  _isInMotion;
    private bool  _isInitialized;

    public System.Action OnAlertStarted;
    public System.Action OnAlertCompleted;
    public System.Action OnHitReactionStarted;
    public System.Action OnHitReactionCompleted;
    public System.Action OnDeathCompleted;
    public System.Action OnMeleeHit;
    public System.Action<int> OnFootstep;

    private void Awake()
    {
        if (_animator  == null) _animator  = GetComponent<Animator>();
        if (_agent      == null) _agent      = GetComponent<NavMeshAgent>();
        if (_state      == null) _state      = GetComponent<EnemyState>();
        if (_navigation == null) _navigation = GetComponent<EnemyNavigation>();
    }

    private void Start()
    {
        TryReadClipLengths();

        if (_state != null)
        {
            _state.OnAlertedChanged        += SetAlerted;
            _state.OnIsFiringChanged       += SetShooting;
            _state.OnIsReloadingChanged    += OnReloadingChanged;
            _state.OnIsMeleeAttackingChanged += OnMeleeAttackingChanged;
            _state.OnIsDeadChanged         += SetDead;
        }

        _isInitialized = true;
    }

    private void OnDestroy()
    {
        if (_state != null)
        {
            _state.OnAlertedChanged        -= SetAlerted;
            _state.OnIsFiringChanged       -= SetShooting;
            _state.OnIsReloadingChanged    -= OnReloadingChanged;
            _state.OnIsMeleeAttackingChanged -= OnMeleeAttackingChanged;
            _state.OnIsDeadChanged         -= SetDead;
        }
    }

    private void OnReloadingChanged(bool isReloading)
    {
        var ranged = GetComponent<RangedCombatModule>();
        float duration = ranged != null ? ranged.ReloadTime : 3f;
        SetReloading(isReloading, duration);
    }

    private void OnMeleeAttackingChanged(bool isAttacking)
    {
        var melee = GetComponent<MeleeCombatModule>();
        float duration = melee != null ? melee.AttackDuration : 3f;
        bool inMotion = _isInMotion;
        SetMeleeAttacking(isAttacking, duration, inMotion);
    }

    private void OnValidate()
    {
        if (_animator  == null) _animator  = GetComponent<Animator>();
        if (_agent      == null) _agent      = GetComponent<NavMeshAgent>();
        if (_state      == null) _state      = GetComponent<EnemyState>();
        if (_navigation == null) _navigation = GetComponent<EnemyNavigation>();
    }

    private void TryReadClipLengths()
    {
        var controller = _animator.runtimeAnimatorController;
        if (controller == null) return;

        var reloadClip = controller.animationClips.FirstOrDefault(c => c.name == _reloadClipName);
        if (reloadClip != null) _reloadClipLength = reloadClip.length;

        var meleeClip = controller.animationClips.FirstOrDefault(c => c.name == _meleeClipName);
        if (meleeClip != null) _meleeClipLength = meleeClip.length;
    }

    private void Update()
    {
        if (!_isInitialized) return;
        UpdateMovement();
        UpdateIdleVariations();
    }

    private void UpdateMovement()
    {
        if (_agent == null) return;

        float raw = _agent.velocity.magnitude;
        float maxSpeed = Mathf.Max(_agent.speed, 0.001f);
        float target = Mathf.Clamp01(raw / maxSpeed) * _speedMultiplier;

        if (target < _minSpeedThreshold) target = 0f;

        _cachedSpeed = Mathf.Lerp(_cachedSpeed, target, _speedSmoothTime * Time.deltaTime * 10f);
        _animator.SetFloat(P.Speed, _cachedSpeed);

        bool moving = raw > _minSpeedThreshold;
        _isInMotion = moving;

        if (moving) _wasMoving = true;
        else if (_wasMoving) _wasMoving = false;
    }

    private void UpdateIdleVariations()
    {
        if (_cachedSpeed > 0.05f) return;

        _idleTimer += Time.deltaTime;
        if (_idleTimer >= _idleVariationInterval)
        {
            _idleTimer = 0f;
            if (Random.value < _idleVariationChance)
            {
                float idle = Random.Range(0, _idleVariationsCount);
                _animator.SetFloat(P.RandomIdleF, idle);
            }
        }
    }

    public void PlayAlert()
    {
        if (!_isInitialized) return;
        _animator.SetTrigger(P.Alert);
    }

    public void PlaySearch()
    {
        if (!_isInitialized) return;
        _animator.SetTrigger(P.Search);
    }

    public void PlayWinning()
    {
        if (!_isInitialized) return;
        _animator.SetInteger(P.WinNumber, Random.Range(0, _amountWinAnimations));
        _animator.SetTrigger(P.Winning);
    }

    public void PlayHit()
    {
        if (!_isInitialized) return;
        _animator.SetTrigger(P.Hit);
    }

    public void PlayDeath()
    {
        // смерть управляется через SetDead
    }

    public void SetAiming(bool isAiming)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.Aiming, isAiming);
    }

    public void SetShooting(bool isShooting)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.Shooting, isShooting);
    }

    public void SetReloading(bool isReloading, float reloadDuration)
    {
        if (!_isInitialized) return;
        float speed = reloadDuration > 0f ? _reloadClipLength / reloadDuration : 1f;
        _animator.SetFloat(P.ReloadSpeed, speed);
        _animator.SetBool(P.Reloading, isReloading);
    }

    public void SetMeleeAttacking(bool isAttacking, float attackDuration, bool inMotion)
    {
        if (!_isInitialized) return;

        // 1 = Stationary, 0 = InMotion
        _animator.SetInteger(P.MeleeAttackType, inMotion ? 0 : 1);

        float speed = attackDuration > 0f ? _meleeClipLength / attackDuration : 1f;
        _animator.SetFloat(P.MeleeSpeed, speed);
        _animator.SetBool(P.MeleeAttacking, isAttacking);
    }

    public void SetAlerted(bool isAlerted)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.IsAlerted, isAlerted);
    }

    public void SetDead(bool isDead)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.IsDead, isDead);
    }

    public void ResetAnimator()
    {
        if (!_isInitialized) return;

        _animator.SetBool(P.IsDead, false);
        _animator.SetBool(P.IsAlerted, false);
        _animator.SetBool(P.HasAlerted, false);
        _animator.SetBool(P.Aiming, false);
        _animator.SetBool(P.Reloading, false);
        _animator.SetBool(P.MeleeAttacking, false);
        _animator.SetBool(P.Shooting, false);
        _animator.SetFloat(P.Speed, 0f);

        _animator.ResetTrigger(P.Alert);
        _animator.ResetTrigger(P.Hit);
        _animator.ResetTrigger(P.Reload);
        _animator.ResetTrigger(P.Search);
        _animator.ResetTrigger(P.Winning);

        _cachedSpeed = 0f;
        _idleTimer = 0f;
    }

    public void OnAlertStart()
    {
        if (_state != null) _state.IsAlertAnimationPlaying = true;
        OnAlertStarted?.Invoke();
    }

    public void OnAlertComplete()
    {
        if (_state != null) _state.IsAlertAnimationPlaying = false;
        SetAiming(true);
        _animator.SetBool(P.HasAlerted, true);
        OnAlertCompleted?.Invoke();
    }

    public void OnHitReactionStart()     => OnHitReactionStarted?.Invoke();
    public void OnHitReactionComplete()  => OnHitReactionCompleted?.Invoke();
    public void OnDeathAnimationComplete() => OnDeathCompleted?.Invoke();

    public void OnMeleeAttackHit()     => OnMeleeHit?.Invoke();
    public void OnMeleeAttackComplete() { /* опционально */ }

    public void OnFootstepLeft()  => OnFootstep?.Invoke(0);
    public void OnFootstepRight() => OnFootstep?.Invoke(1);

    public void OnReloadComplete()
    {
        _animator.SetBool(P.Reloading, false);
    }
}