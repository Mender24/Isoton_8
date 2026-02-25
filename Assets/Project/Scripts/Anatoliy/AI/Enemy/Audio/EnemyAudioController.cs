using System.Collections.Generic;
using UnityEngine;

public class EnemyAudioController : MonoBehaviour, IEnemyAudio
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip _detectionClip;
    [SerializeField] private AudioClip _alertClip;
    [SerializeField] private AudioClip _deathClip;
    [SerializeField] private AudioClip _reloadClip;
    [SerializeField] private List<AudioClip> _attackClips  = new();
    [SerializeField] private List<AudioClip> _hitClips     = new();
    [SerializeField] private List<AudioClip> _footstepClips = new();

    [Header("Cooldowns")]
    [SerializeField] private float _detectionCooldown = 30f;

    private float _detectionTimer;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_detectionTimer > 0f)
            _detectionTimer -= Time.deltaTime;
    }

    public void PlayDetectionSound()
    {
        if (_detectionTimer > 0f) return;
        _detectionTimer = _detectionCooldown;
        Play(_detectionClip);
    }

    public void PlayAlertSound()    => Play(_alertClip);
    public void PlayDeathSound()    => Play(_deathClip);
    public void PlayReloadSound()   => Play(_reloadClip);

    public void PlayAttackSound()   => PlayRandom(_attackClips);
    public void PlayHitSound()      => PlayRandom(_hitClips);

    public void PlayFootstep(int foot) => PlayRandom(_footstepClips);

    private void Play(AudioClip clip)
    {
        if (_audioSource == null || clip == null) return;
        _audioSource.PlayOneShot(clip);
    }

    private void PlayRandom(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        Play(clips[Random.Range(0, clips.Count)]);
    }
}
