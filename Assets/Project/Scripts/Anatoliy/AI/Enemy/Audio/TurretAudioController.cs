using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TurretAudioController : MonoBehaviour, IEnemyAudio
{
    [Header("Audio Sources")]
    [Tooltip("Основной источник (смерть, перезарядка и прочее).")]
    [SerializeField] private AudioSource _mainSource;

    [Tooltip("Источник для звука выстрела.")]
    [SerializeField] private AudioSource _attackSource;

    [Tooltip("Источник лупа поворота (Loop = true, Play On Awake = false).")]
    [SerializeField] private AudioSource _rotationSource;

    [Header("Clips Attack")]
    [SerializeField] private List<AudioClip> _shootClips = new();
    [SerializeField] private float _shootPitchVariation = 0.1f;

    [Header("Clips Death")]
    [SerializeField] private AudioClip _explosionClip;

    [Header("Clips Reload (опционально)")]
    [SerializeField] private AudioClip _reloadClip;

    [Header("Rotation Sound")]
    [Tooltip("Скорость нарастания/убывания громкости лупа поворота.")]
    [SerializeField] private float _rotationFadeSpeed = 6f;

    [Tooltip("Угловая скорость (deg/сек), ниже которой считается что туррель не вращается.")]
    [SerializeField] private float _minRotationSpeed = 1f;

    private Quaternion _prevRotation;
    private float      _basePitch;

    private void Awake()
    {
        if (_mainSource   == null) _mainSource   = GetComponent<AudioSource>();
        if (_attackSource == null) _attackSource = _mainSource;

        if (_rotationSource != null)
        {
            _rotationSource.loop        = true;
            _rotationSource.playOnAwake = false;
            _rotationSource.volume      = 0f;
        }

        _basePitch   = _attackSource != null ? _attackSource.pitch : 1f;
        _prevRotation = transform.rotation;
    }

    private void Update()
    {
        if (_rotationSource == null) return;

        float angularSpeed = Quaternion.Angle(_prevRotation, transform.rotation) / Time.deltaTime;
        _prevRotation = transform.rotation;

        bool isRotating = angularSpeed > _minRotationSpeed;

        if (isRotating && !_rotationSource.isPlaying)
            _rotationSource.Play();

        float targetVolume = isRotating ? 1f : 0f;
        _rotationSource.volume = Mathf.MoveTowards(
            _rotationSource.volume, targetVolume, _rotationFadeSpeed * Time.deltaTime);

        if (!isRotating && _rotationSource.volume <= 0f && _rotationSource.isPlaying)
            _rotationSource.Stop();
    }

    public void PlayAttackSound()
    {
        if (_shootClips == null || _shootClips.Count == 0 || _attackSource == null) return;
        _attackSource.pitch = _basePitch + Random.Range(-_shootPitchVariation, _shootPitchVariation);
        _attackSource.PlayOneShot(_shootClips[Random.Range(0, _shootClips.Count)]);
    }

    public void PlayDeathSound()
    {
        if (_mainSource == null || _explosionClip == null) return;
        if (_rotationSource != null) _rotationSource.Stop();
        _mainSource.PlayOneShot(_explosionClip);
    }

    public void PlayReloadSound()
    {
        if (_mainSource == null || _reloadClip == null) return;
        _mainSource.PlayOneShot(_reloadClip);
    }

    public void PlayDetectionSound()    { }
    public void PlayAlertSound()        { }
    public void PlayHitSound()          { }
    public void PlayGrenadeOpenSound()  { }
    public void PlayGrenadeVoiceLine()  { }
    public void PlayRandomYap()         { }
    public void PlayFootstep(int foot)  { }
    public void PlayNamedSound(string soundName)  { }
    public void PlayRandomNamedSound()  { }
    public List<CellAudioClip> GetGrenadeBounceClips() { return null; }
}
