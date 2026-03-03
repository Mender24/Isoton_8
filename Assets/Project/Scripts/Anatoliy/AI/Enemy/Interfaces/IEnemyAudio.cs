public interface IEnemyAudio
{
    void PlayDetectionSound();
    void PlayAlertSound();
    void PlayAttackSound();
    void PlayReloadSound();
    void PlayHitSound();
    void PlayDeathSound();
    void PlayFootstep(int foot);
    void PlayNamedSound(string soundName);
    void PlayRandomNamedSound();
}
