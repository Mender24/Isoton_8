public interface IEnemyAudio
{
    void PlayDetectionSound();
    void PlayAttackSound();
    void PlayDeathSound();
    void PlayHitSound();
    void PlayAlertSound();
    void PlayFootstep(int foot);
    void PlayReloadSound();
}
