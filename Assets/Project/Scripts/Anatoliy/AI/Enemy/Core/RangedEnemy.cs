using UnityEngine;

[RequireComponent(typeof(RangedCombatModule))]
public class RangedEnemy : EnemyBase
{
    private RangedCombatModule _rangedCombat;
    private GrenadeThrowModule _grenadeThrow;
    private BasicEnemyAnimator _basicAnimator;

    protected override void Awake()
    {
        base.Awake();
        _rangedCombat  = GetComponent<RangedCombatModule>();
        _grenadeThrow  = GetComponent<GrenadeThrowModule>();
        _basicAnimator = GetComponent<BasicEnemyAnimator>();
    }

    protected override void OnInitialized()
    {
        _rangedCombat.Initialize(PlayerTransform);
        _grenadeThrow?.Initialize(PlayerTransform);

        if (_basicAnimator != null)
        {
            _basicAnimator.OnHitReactionStarted   += OnHitReactionStarted;
            _basicAnimator.OnHitReactionCompleted += OnHitReactionCompleted;
        }
    }

    private void OnDestroy()
    {
        if (_basicAnimator != null)
        {
            _basicAnimator.OnHitReactionStarted   -= OnHitReactionStarted;
            _basicAnimator.OnHitReactionCompleted -= OnHitReactionCompleted;
        }
    }

    private void OnHitReactionStarted()
    {
        _rangedCombat.SetPaused(true);
        _grenadeThrow?.Cancel();
    }

    private void OnHitReactionCompleted() => _rangedCombat.SetPaused(false);

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;
        _rangedCombat.Tick(Time.deltaTime);
        _grenadeThrow?.Tick(Time.deltaTime);
    }

    public override bool CanAttack()  => _rangedCombat.CanShoot;
    public override void StartAttack() => _rangedCombat.StartFire();

    public IRangedCombat RangedCombat => _rangedCombat;
}
