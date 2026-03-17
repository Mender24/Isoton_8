using System.Linq;
using UnityEngine;

/// <summary>
/// Упрощённый аниматор для ползающего врага.
/// Использует EnemyNavigation.CurrentSpeed.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CrawlerAnimator : MonoBehaviour, IEnemyAnimator
{
    [Header("References")]
    [SerializeField] private Animator        _animator;
    [SerializeField] private EnemyState      _state;
    [SerializeField] private EnemyNavigation _navigation;

    [Header("Movement")]
    [SerializeField] private float _speedSmoothTime    = 0.15f;
    [SerializeField] private float _minSpeedThreshold  = 0.1f;
    [SerializeField] private float _speedMultiplier    = 1f;

    [Header("Clip names")]
    [SerializeField] private string _meleeClipName = "AttackRightClaws1Creature_RM";

    private static class P
    {
        public const string Speed          = "Speed";
        public const string IsDead         = "IsDead";
        public const string MeleeAttack    = "MeleeAttack";
        public const string MeleeAttacking = "MeleeAttacking";
        public const string MeleeSpeed     = "MeleeSpeed";
        public const string MeleeAttackType = "MeleeAttackType";
        public const string Hit            = "Hit";
        public const string IsAlerted      = "IsAlerted";
    }

    private float _cachedSpeed;
    private float _meleeClipLength = 3.2f;
    private bool  _isInitialized;

    public System.Action OnMeleeHit;

    private void Awake()
    {
        if (_animator   == null) _animator   = GetComponent<Animator>();
        if (_state      == null) _state      = GetComponent<EnemyState>();
        if (_navigation == null) _navigation = GetComponent<EnemyNavigation>();
    }

    private void Start()
    {
        TryReadMeleeClipLength();

        if (_state != null)
        {
            _state.OnIsDeadChanged       += SetDead;
            _state.OnAlertedChanged      += SetAlerted;
        }

        _isInitialized = true;
    }

    private void OnDestroy()
    {
        if (_state != null)
        {
            _state.OnIsDeadChanged  -= SetDead;
            _state.OnAlertedChanged -= SetAlerted;
        }
    }

    private void TryReadMeleeClipLength()
    {
        var controller = _animator.runtimeAnimatorController;
        if (controller == null) return;
        var clip = controller.animationClips.FirstOrDefault(c => c.name == _meleeClipName);
        if (clip != null) _meleeClipLength = clip.length;
    }

    private void Update()
    {
        if (!_isInitialized) return;
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        float rawSpeed = _navigation != null ? _navigation.CurrentSpeed : 0f;
        float maxSpeed = _navigation != null ? _navigation.RunSpeed     : 1f;
        float target   = Mathf.Clamp01(rawSpeed / Mathf.Max(maxSpeed, 0.001f)) * _speedMultiplier;

        if (target < _minSpeedThreshold) target = 0f;

        _cachedSpeed = Mathf.Lerp(_cachedSpeed, target, _speedSmoothTime * Time.deltaTime * 10f);
        _animator.SetFloat(P.Speed, _cachedSpeed);
    }

    public void SetMeleeAttacking(bool isAttacking, float attackDuration, bool inMotion)
    {
        if (!_isInitialized) return;
        _animator.SetFloat(P.MeleeAttackType, inMotion ? 0 : 1);
        float speed = attackDuration > 0f ? _meleeClipLength / attackDuration : 1f;
        _animator.SetFloat(P.MeleeSpeed, speed);
        _animator.SetBool(P.MeleeAttacking, isAttacking);
    }

    public void SetDead(bool isDead)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.IsDead, isDead);
    }

    public void SetAlerted(bool isAlerted)
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.IsAlerted, isAlerted);
    }

    public void PlayHit()
    {
        if (!_isInitialized) return;
        _animator.SetTrigger(P.Hit);
    }

    public void ResetAnimator()
    {
        if (!_isInitialized) return;
        _animator.SetBool(P.IsDead, false);
        _animator.SetBool(P.IsAlerted, false);
        _animator.SetBool(P.MeleeAttacking, false);
        _animator.SetFloat(P.Speed, 0f);
        _animator.ResetTrigger(P.Hit);
        _cachedSpeed = 0f;
    }

    public void PlayAlert()    { }
    public void PlaySearch()   { }
    public void PlayWinning()  { }
    public void PlayDeath()    { }
    public void SetAiming(bool isAiming)                              { }
    public void SetShooting(bool isShooting)                          { }
    public void SetReloading(bool isReloading, float reloadDuration)  { }
    public void TriggerGrenadeWindUp(float windUpDuration)            { }
    public void TriggerGrenadeThrow(float throwDuration)              { }
    public void CancelGrenadeThrow()                                  { }
    public void ResetSearch()                                         { }

    public void OnMeleeAttackHit()      => OnMeleeHit?.Invoke();
    public void OnMeleeAttackComplete() { }
    public void OnFootstepLeft()        { }
    public void OnFootstepRight()       { }
}
