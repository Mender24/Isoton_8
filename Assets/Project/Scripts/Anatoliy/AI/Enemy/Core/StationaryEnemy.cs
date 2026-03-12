using System.Linq;
using UnityEngine;

/// <summary>
/// Стационарный страж. Стоит на месте, стреляет по игроку.
/// При получении урона приседает за укрытием на <see cref="_coverDuration"/> секунд,
/// затем встаёт и снова стреляет. Если игрок подходит ближе <see cref="_closeRangeThreshold"/>
/// и видит врага враг не приседает и стреляет постоянно.
/// </summary>
[RequireComponent(typeof(RangedCombatModule))]
public class StationaryEnemy : EnemyBase
{
    private enum GuardState { Idle, Shooting, Crouching }

    [Header("Stationary Guard")]
    [Tooltip("Время в приседании после попадания, прежде чем снова встать.")]
    [SerializeField] private float _coverDuration = 2.5f;

    [Tooltip("Если игрок ближе этого расстояния враг никогда не приседает и стреляет постоянно.")]
    [SerializeField] private float _closeRangeThreshold = 4f;

    [Header("Crouch Animation")]
    [Tooltip("Имя клипа анимации приседания (для расчёта скорости).")]
    [SerializeField] private string _crouchClipName = "CrouchDown";

    private float _crouchClipLength = 1f;

    private RangedCombatModule _rangedCombat;
    private BasicEnemyAnimator _basicAnimator;
    private Animator           _animator;

    private GuardState _guardState = GuardState.Idle;
    private float      _coverTimer;

    protected override void Awake()
    {
        base.Awake();
        _rangedCombat  = GetComponent<RangedCombatModule>();
        _basicAnimator = GetComponent<BasicEnemyAnimator>();
        _animator      = GetComponent<Animator>();
    }

    protected override void OnInitialized()
    {
        _rangedCombat.Initialize(PlayerTransform);
        Navigation.Stop();
        Navigation.Agent.updateRotation = false;
        TryReadClipLengths();
    }

    private void TryReadClipLengths()
    {
        if (_animator == null) return;
        var controller = _animator.runtimeAnimatorController;
        if (controller == null) return;

        var crouchClip = controller.animationClips.FirstOrDefault(c => c.name == _crouchClipName);
        if (crouchClip != null) _crouchClipLength = crouchClip.length;
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;

        _rangedCombat.Tick(Time.deltaTime);

        if (State.PlayerIsSeen && !State.IsAlerted)
            OnPlayerDetected();

        if (_guardState != GuardState.Crouching && State.IsAlerted)
            RotateTowardsPlayer();

        switch (_guardState)
        {
            case GuardState.Idle:
                if (State.PlayerDetected && State.PlayerIsSeen)
                    EnterShooting();
                break;

            case GuardState.Shooting:
                if (!State.PlayerDetected)
                {
                    EnterIdle();
                    break;
                }

                if (State.PlayerIsSeen && !_rangedCombat.IsFiring
                    && _rangedCombat.CanShoot && !State.IsReloading)
                {
                    _rangedCombat.StartFire();
                }
                break;

            case GuardState.Crouching:

                if (IsPlayerClose() && State.PlayerIsSeen)
                {
                    ExitCrouch();
                    break;
                }
                _coverTimer -= Time.deltaTime;
                if (_coverTimer <= 0f)
                    ExitCrouch();
                break;
        }
    }

    protected override void OnDamaged(float amount, GameObject source)
    {
        base.OnDamaged(amount, source);

        if (_guardState == GuardState.Crouching) return;

        if (IsPlayerClose()) return;

        EnterCrouch();
    }

    private void EnterIdle()
    {
        _guardState = GuardState.Idle;
        _rangedCombat.StopFire();
        _basicAnimator?.SetAiming(false);
    }

    private void EnterShooting()
    {
        _guardState = GuardState.Shooting;
        _basicAnimator?.SetAiming(true);
        _rangedCombat.StartFire();
    }

    private void EnterCrouch()
    {
        _guardState = GuardState.Crouching;
        _coverTimer = _coverDuration;
        _rangedCombat.StopFire();
        _basicAnimator?.SetAiming(false);
        SetCrouching(true);
    }

    private void ExitCrouch()
    {
        SetCrouching(false);

        if (State.PlayerDetected && State.PlayerIsSeen)
            EnterShooting();
        else
            EnterIdle();
    }

    private bool IsPlayerClose() =>
        Perception.GetDistanceToPlayer() <= _closeRangeThreshold;

    private void RotateTowardsPlayer()
    {
        if (PlayerTransform == null) return;

        Vector3 dir = PlayerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target,
            Navigation.RotationSpeed * 10f * Time.deltaTime);
    }

    private void SetCrouching(bool crouch)
    {
        if (_animator == null) return;

        if (crouch)
        {
            float speed = _coverDuration > 0f ? _crouchClipLength / _coverDuration : 1f;
            _animator.SetFloat("CrouchSpeed", speed);
            _animator.SetTrigger("CrouchDown");
        }
        else
        {
            _animator.SetTrigger("StandUp");
        }
    }

    public override bool CanAttack()   => _rangedCombat.CanShoot;
    public override void StartAttack() => _rangedCombat.StartFire();
}
