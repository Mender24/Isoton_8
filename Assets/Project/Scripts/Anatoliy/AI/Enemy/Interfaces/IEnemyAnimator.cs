public interface IEnemyAnimator
{
    void PlayAlert();
    void PlaySearch();
    void PlayWinning();
    void PlayHit();
    void PlayDeath();

    void SetAiming(bool isAiming);
    void SetShooting(bool isShooting);
    void SetReloading(bool isReloading, float reloadDuration);
    void SetMeleeAttacking(bool isAttacking, float attackDuration, bool inMotion);

    void SetAlerted(bool isAlerted);
    void SetDead(bool isDead);

    void ResetAnimator();
}
