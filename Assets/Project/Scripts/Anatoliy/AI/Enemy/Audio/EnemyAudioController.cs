using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioController : MonoBehaviour, IEnemyAudio
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _audioSource; // Main audio source used as fallback if other is empty
    [SerializeField] private AudioSource _stepAudioSource;
    [SerializeField] private AudioSource _attackAudioSource;
    [SerializeField] private AudioSource _talkAudioSource;

    [Header("Clips")]
    [SerializeField] private List<CellAudioClip> _detectionClips = new();
    [SerializeField] private List<CellAudioClip> _alertClips = new();
    [SerializeField] private List<CellAudioClip> _deathClips = new();
    [SerializeField] private List<CellAudioClip> _reloadClips = new();
    [SerializeField] private List<CellAudioClip> _grenadeOpenClips = new();
    [SerializeField] private List<CellAudioClip> _grenadeBounceClips = new();
    [SerializeField] private List<CellAudioClip> _attackClips = new();
    [SerializeField] private List<CellAudioClip> _hitClips = new();
    [SerializeField] private List<CellAudioClip> _footstepClips = new();

    [Header("Talk / Named Sounds")]
    [SerializeField] private List<CellAudioClip> _namedClips = new();
    
    [Header("Footstep Settings")]
    [SerializeField] private float _stepVolume = -1f; // -1 = use per-clip volume from CellAudioClip

    [Header("Attack Settings")]
    [SerializeField] private float _maxAttackPitchVariation = 0.2f;

    [Header("Cooldowns")]
    [SerializeField] private float _detectionCooldown = 30f;

    private float _detectionTimer;
    private float _baseAttackPitch;
    private Dictionary<string, CellAudioClip> _namedClipsDict;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_stepAudioSource   == null) _stepAudioSource   = _audioSource;
        if (_attackAudioSource == null) _attackAudioSource = _audioSource;
        if (_talkAudioSource   == null) _talkAudioSource   = _audioSource;

        _baseAttackPitch = _attackAudioSource.pitch;

        _namedClipsDict = new Dictionary<string, CellAudioClip>();
        foreach (var cell in _namedClips)
            if (cell.AudioClip != null)
                _namedClipsDict[cell.AudioClip.name] = cell;
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
        PlayRandom(_audioSource, _detectionClips);
    }

    public void PlayAlertSound()  => PlayRandom(_audioSource, _alertClips);
    public void PlayDeathSound()  => PlayRandom(_audioSource, _deathClips);
    public void PlayReloadSound() => PlayRandom(_audioSource, _reloadClips);
    public void PlayHitSound()    => PlayRandom(_audioSource, _hitClips);
    public void PlayGrenadeOpenSound() => PlayRandom(_audioSource, _grenadeOpenClips);

    public void PlayAttackSound()
    {
        if (_attackClips == null || _attackClips.Count == 0) return;
        _attackAudioSource.pitch = _baseAttackPitch + Random.value * _maxAttackPitchVariation;
        _attackClips[Random.Range(0, _attackClips.Count)].PlayAudioClipOneShot(_attackAudioSource);
    }

    public void PlayFootstep(int foot)
    {
        if (_footstepClips == null || _footstepClips.Count == 0) return;
        _footstepClips[Random.Range(0, _footstepClips.Count)].PlayAudioClipOneShot(_stepAudioSource, _stepVolume);
    }

    public void PlayNamedSound(string soundName)
    {
        if (!_namedClipsDict.TryGetValue(soundName, out var cell)) return;
        cell.PlayAudioClip(_talkAudioSource);
    }

    public List<CellAudioClip> GetGrenadeBounceClips() => _grenadeBounceClips;

    public void PlayRandomNamedSound()
    {
        if (_namedClips == null || _namedClips.Count == 0) return;
        _namedClips[Random.Range(0, _namedClips.Count)].PlayAudioClip(_talkAudioSource);
    }

    private void Play(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip);
    }

    private void PlayRandom(AudioSource source, List<CellAudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        clips[Random.Range(0, clips.Count)].PlayAudioClipOneShot(source);
    }
}
