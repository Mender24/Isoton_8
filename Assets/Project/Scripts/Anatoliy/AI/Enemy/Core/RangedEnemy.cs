using UnityEngine;

[RequireComponent(typeof(RangedCombatModule))]
public class RangedEnemy : EnemyBase
{
    private RangedCombatModule _rangedCombat;

    protected override void Awake()
    {
        base.Awake();
        _rangedCombat = GetComponent<RangedCombatModule>();
    }

    protected override void OnInitialized()
    {
        _rangedCombat.Initialize(PlayerTransform);
    }

    private void Update()
    {
        if (!State.IsActivated || State.IsDead) return;
        _rangedCombat.Tick(Time.deltaTime);
    }

    public override bool CanAttack()  => _rangedCombat.CanShoot;
    public override void StartAttack() => _rangedCombat.StartFire();

    public IRangedCombat RangedCombat => _rangedCombat;
}
