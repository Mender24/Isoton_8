using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioController : MonoBehaviour, IEnemyAudio
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _stepAudioSource;
    [SerializeField] private AudioSource _attackAudioSource;
    [SerializeField] private AudioSource _talkAudioSource;

    [Header("Clips")]
    [SerializeField] private List<CellAudioClip> _detectionClips = new();
    [SerializeField] private List<CellAudioClip> _alertClips = new();
    [SerializeField] private List<CellAudioClip> _deathClips = new();
    [SerializeField] private List<CellAudioClip> _reloadClips = new();
    [SerializeField] private List<CellAudioClip> _grenadeOpenClips = new();
    [SerializeField] private List<CellAudioClip> _grenadeVoicelineClips = new();
    [SerializeField] private List<CellAudioClip> _grenadeBounceClips = new();
    [SerializeField] private List<CellAudioClip> _attackClips = new();
    [SerializeField] private List<CellAudioClip> _hitClips = new();
    [SerializeField] private List<CellAudioClip> _footstepClips = new();

    [Header("Named Sounds")]
    [SerializeField] private List<NamedAudioClip> _namedClips = new();

    [Header("Combat Yapping")]
    [SerializeField] private List<YapGroup> _yapGroups = new();
    [SerializeField] private float _globalYapCooldown = 5f;

    [Header("Footstep Settings")]
    [SerializeField] private float _stepVolume = -1f;
    [SerializeField] private float _minStepInterval = 0.2f;

    [Header("Attack Settings")]
    [SerializeField] private float _maxAttackPitchVariation = 0.2f;

    [Header("Cooldowns")]
    [SerializeField] private float _detectionCooldown = 30f;

    private static float _sharedYapTimer;

    private float _detectionTimer;
    private float _globalYapTimer;
    private float _stepTimer;
    private float _baseAttackPitch;
    private Dictionary<string, CellAudioClip> _namedClipsDict;
    private Dictionary<List<CellAudioClip>, int> _lastPlayedIndices = new();

    private void Start()
    {
        FootstepManager.Instance?.Register(transform);
    }

    private void OnDestroy()
    {
        FootstepManager.Instance?.Unregister(transform);
    }

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_stepAudioSource   == null) _stepAudioSource   = _audioSource;
        if (_attackAudioSource == null) _attackAudioSource = _audioSource;
        if (_talkAudioSource   == null) _talkAudioSource   = _audioSource;

        _baseAttackPitch = _attackAudioSource.pitch;

        _namedClipsDict = new Dictionary<string, CellAudioClip>();
        foreach (var entry in _namedClips)
            if (entry.Clip?.AudioClip != null)
                _namedClipsDict[entry.Name] = entry.Clip;
    }

    private void Update()
    {
        if (_detectionTimer > 0f) _detectionTimer -= Time.deltaTime;
        if (_globalYapTimer  > 0f) _globalYapTimer  -= Time.deltaTime;
        if (_sharedYapTimer  > 0f) _sharedYapTimer  -= Time.deltaTime;
        if (_stepTimer       > 0f) _stepTimer       -= Time.deltaTime;

        foreach (var group in _yapGroups)
            if (group.CooldownTimer > 0f)
                group.CooldownTimer -= Time.deltaTime;
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

    public void PlayGrenadeOpenSound()  => PlayRandom(_audioSource, _grenadeOpenClips);
    public void PlayGrenadeVoiceLine()  => PlayRandom(_audioSource, _grenadeVoicelineClips);

    public void PlayAttackSound()
    {
        if (_attackClips == null || _attackClips.Count == 0) return;
        _attackAudioSource.pitch = _baseAttackPitch + Random.value * _maxAttackPitchVariation;
        _attackClips[Random.Range(0, _attackClips.Count)].PlayAudioClipOneShot(_attackAudioSource);
    }

    public void PlayFootstep(int foot)
    {
        if (_footstepClips == null || _footstepClips.Count == 0) return;
        if (FootstepManager.Instance != null && !FootstepManager.Instance.CanPlayFootstep(transform)) return;
        if (_stepTimer > 0f) return;
        _stepTimer = _minStepInterval;
        _footstepClips[Random.Range(0, _footstepClips.Count)].PlayAudioClipOneShot(_stepAudioSource, _stepVolume);
    }

    public void PlayNamedSound(string soundName)
    {
        if (!_namedClipsDict.TryGetValue(soundName, out var clip)) return;
        clip.PlayAudioClip(_talkAudioSource);
    }

    public void PlayRandomNamedSound()
    {
        if (_namedClips == null || _namedClips.Count == 0) return;
        _namedClips[Random.Range(0, _namedClips.Count)].Clip?.PlayAudioClip(_talkAudioSource);
    }

    public bool PlayRandomYap()
    {
        if (_globalYapTimer > 0f || _sharedYapTimer > 0f || _yapGroups == null || _yapGroups.Count == 0) return false;

        float totalWeight = 0f;
        foreach (var group in _yapGroups)
            if (group.CooldownTimer <= 0f && group.Clips != null && group.Clips.Count > 0)
                totalWeight += group.Weight;

        if (totalWeight <= 0f) return false;

        float roll = Random.Range(0f, totalWeight);
        float accumulated = 0f;
        YapGroup picked = null;
        foreach (var group in _yapGroups)
        {
            if (group.CooldownTimer > 0f || group.Clips == null || group.Clips.Count == 0) continue;
            accumulated += group.Weight;
            if (roll <= accumulated) { picked = group; break; }
        }

        if (picked == null) return false;

        picked.Clips[Random.Range(0, picked.Clips.Count)].PlayAudioClip(_talkAudioSource);
        picked.CooldownTimer = picked.GroupCooldown;
        _globalYapTimer = _globalYapCooldown;
        _sharedYapTimer = _globalYapCooldown;
        return true;
    }

    public List<CellAudioClip> GetGrenadeBounceClips() => _grenadeBounceClips;

    private void Play(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip);
    }

    private void PlayRandom(AudioSource source, List<CellAudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;

        int lastIndex = _lastPlayedIndices.TryGetValue(clips, out int cached) ? cached : -1;
        int randInd;
        do { randInd = Random.Range(0, clips.Count); }
        while (clips.Count > 1 && randInd == lastIndex);

        clips[randInd].PlayAudioClipOneShot(source);
        _lastPlayedIndices[clips] = randInd;
    }
}
