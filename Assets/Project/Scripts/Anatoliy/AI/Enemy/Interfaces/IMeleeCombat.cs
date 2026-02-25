public interface IMeleeCombat
{
    bool CanAttack { get; }
    bool IsAttacking { get; }
    float AttackRange { get; }

    bool IsInRange();
    void StartAttack();
    void ExecuteHit();

    void Tick(float deltaTime);
}
