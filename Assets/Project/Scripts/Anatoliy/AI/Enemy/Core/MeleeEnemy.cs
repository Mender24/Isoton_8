using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeCombatModule))]
public class MeleeEnemy : EnemyBase
{
    [SerializeField] private float _hitReactionDuration = 0.6f;

    private MeleeCombatModule  _meleeCombat;
    private BasicEnemyAnimator _basicAnimator;
    private Coroutine          _hitReactCoroutine;

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
            _basicAnimator.OnMeleeHit += _meleeCombat.ExecuteHit;
    }

    private void OnDestroy()
    {
        if (_basicAnimator != null)
            _basicAnimator.OnMeleeHit -= _meleeCombat.ExecuteHit;
    }

    protected override void OnDamaged(float amount, GameObject source)
    {
        base.OnDamaged(amount, source);

        if (State.IsMeleeAttacking) return;

        if (_hitReactCoroutine != null) StopCoroutine(_hitReactCoroutine);
        _hitReactCoroutine = StartCoroutine(HitReactRoutine());
    }

    private IEnumerator HitReactRoutine()
    {
        State.IsHitReacting = true;
        Navigation.Stop();

        yield return new WaitForSeconds(_hitReactionDuration);

        State.IsHitReacting = false;
        if (!State.IsMeleeAttacking)
            Navigation.Resume();

        _hitReactCoroutine = null;
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
