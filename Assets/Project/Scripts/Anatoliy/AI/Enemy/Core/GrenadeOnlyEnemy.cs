using UnityEngine;

[RequireComponent(typeof(GrenadeThrowModule))]
public class GrenadeOnlyEnemy : EnemyBase
{
    private GrenadeThrowModule _grenadeThrow;
    private BasicEnemyAnimator _basicAnimator;

    protected override void Awake()
    {
        base.Awake();
        _grenadeThrow = GetComponent<GrenadeThrowModule>();
        _basicAnimator = GetComponent<BasicEnemyAnimator>();
    }

    protected override void OnInitialized()
    {
        _grenadeThrow.Initialize(PlayerTransform);

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

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;
        _grenadeThrow?.Tick(Time.deltaTime);
    }

    private void OnHitReactionStarted()
    {
        _grenadeThrow?.Cancel(); // сбрасывает Phase в Idle и IsThrowingGrenade в false
        Navigation.Stop();
    }

    private void OnHitReactionCompleted()
    {
        Navigation.Resume();
    }

    public override bool CanAttack()   => _grenadeThrow != null && _grenadeThrow.CanThrowGrenade;
    public override void StartAttack() => _grenadeThrow?.StartWindUp();

    public override void FullReset()
    {
        base.FullReset();
        _grenadeThrow?.Reset(PlayerTransform);
    }
}