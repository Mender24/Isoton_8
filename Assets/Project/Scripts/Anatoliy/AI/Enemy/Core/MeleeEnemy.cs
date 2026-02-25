using UnityEngine;

[RequireComponent(typeof(MeleeCombatModule))]
public class MeleeEnemy : EnemyBase
{
    private MeleeCombatModule _meleeCombat;

    protected override void Awake()
    {
        base.Awake();
        _meleeCombat = GetComponent<MeleeCombatModule>();
    }

    protected override void OnInitialized()
    {
        _meleeCombat.Initialize(PlayerTransform);
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
