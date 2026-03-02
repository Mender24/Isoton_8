using System.Collections.Generic;
using UnityEngine;

public class EntitySoundManager : MonoBehaviour
{
    [Header("Step settings")]
    [SerializeField] private AudioSource _stepAudioSource;
    [Space]
    [SerializeField] private float _stepVolume = -1; //Force setting. All volume is equal. -1 if need target settings.
    [SerializeField] private List<CellAudioClip> _stepAudioClips;
    [Space]

    [Header("Fire settings")]
    [SerializeField] private AudioSource _fireAudioSource;
    [Space]
    [SerializeField] private AudioClip _fireAudioClip;
    [SerializeField] private float _maxPitchBust = 0.2f;
    private float _basePitch;
    [Space]

    [Header("Talk settings")]
    [SerializeField] private AudioSource _otherAudioSource;
    [Space]
    [SerializeField] private List<CellAudioClip> _otherAudioClips;
    private Dictionary<string, CellAudioClip> _otherClips;

    private void Awake()
    {
        _basePitch = _fireAudioSource.pitch;

        foreach (var cell in _otherAudioClips)
            _otherClips.Add(cell.AudioClip.name, cell);
    }

    public void PlayStepSound()
    {
        _stepAudioClips[Random.Range(0, _stepAudioClips.Count)].PlayAudioClipOneShot(_stepAudioSource, _stepVolume);
    }

    public void PlayFireSound()
    {
        float randomAdditive = Random.value * _maxPitchBust;
        _fireAudioSource.pitch = _basePitch + randomAdditive;

        _fireAudioSource.PlayOneShot(_fireAudioClip);
    }

    public void PlayOtherSound(string soundName)
    {
        if (!_otherClips.ContainsKey(soundName))
            return;

        _otherClips[soundName].PlayAudioClip(_otherAudioSource);
    }

    public void PlayRandomOtherSound()
    {
        int randomValue = Random.Range(0, _otherAudioClips.Count);
        _otherAudioClips[randomValue].PlayAudioClip(_otherAudioSource);
    }
}
