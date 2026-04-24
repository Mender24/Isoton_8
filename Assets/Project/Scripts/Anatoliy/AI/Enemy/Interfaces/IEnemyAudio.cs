using System.Collections.Generic;

public interface IEnemyAudio
{
    void PlayDetectionSound();
    void PlayAlertSound();
    void PlayAttackSound();
    void PlayReloadSound();
    void PlayHitSound();
    void PlayGrenadeOpenSound();
    void PlayGrenadeVoiceLine();
    void PlayDeathSound();
    void PlayFootstep(int foot);
    void PlayNamedSound(string soundName);
    void PlayRandomNamedSound();
    void PlayRandomYap();
    List<CellAudioClip> GetGrenadeBounceClips();
}
