using UnityEngine;

[RequireComponent(typeof(MeleeCombatModule))]
public class MeleeEnemy : EnemyBase
{
    private MeleeCombatModule  _meleeCombat;
    private BasicEnemyAnimator _basicAnimator;

    protected override void Awake()
    {
        base.Awake();
        _meleeCombat   = GetComponent<MeleeCombatModule>();
        _basicAnimator = GetComponent<BasicEnemyAnimator>();
    }

    protected override void OnInitialized()
    {
        _meleeCombat.Initialize(PlayerTransform);

        if (_basicAnimator != null)
        {
            _basicAnimator.OnMeleeHit             += _meleeCombat.ExecuteHit;
            _basicAnimator.OnHitReactionStarted   += OnHitReactionStarted;
            _basicAnimator.OnHitReactionCompleted += OnHitReactionCompleted;
        }
    }

    private void OnDestroy()
    {
        if (_basicAnimator != null)
        {
            _basicAnimator.OnMeleeHit             -= _meleeCombat.ExecuteHit;
            _basicAnimator.OnHitReactionStarted   -= OnHitReactionStarted;
            _basicAnimator.OnHitReactionCompleted -= OnHitReactionCompleted;
        }
    }

    private void OnHitReactionStarted()
    {
        if (State.IsMeleeAttacking) return;
        State.IsHitReacting = true;
        Navigation.Stop();
    }

    private void OnHitReactionCompleted()
    {
        State.IsHitReacting = false;
        if (!State.IsMeleeAttacking)
            Navigation.Resume();
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;
        _meleeCombat.Tick(Time.deltaTime);
    }

    public override bool CanAttack()   => _meleeCombat.CanAttack;
    public override void StartAttack() => _meleeCombat.StartAttack();

    public IMeleeCombat MeleeCombat => _meleeCombat;

    public bool IsInMeleeRange() => _meleeCombat.IsInRange();
}
