using Unity.Behavior;

public interface IRangedCombat
{
    bool CanShoot { get; }
    bool IsFiring  { get; }
    bool IsReloading { get; }
    float AttackRange { get; }

    void StartFire();
    void StopFire();

    void Tick(float deltaTime);
}
