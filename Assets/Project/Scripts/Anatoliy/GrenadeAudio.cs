using System.Collections.Generic;
using UnityEngine;

public class GrenadeAudio : MonoBehaviour
{
    [SerializeField] private List<CellAudioClip> _fallOnGroundClips;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private AudioSource _grenadeAudioSource;

    private bool _hasPlayedFallSound = false;
    
    void Awake()
    {
        if (_grenadeAudioSource == null)
            _grenadeAudioSource = GetComponent<AudioSource>();
    }

    public void Setup(List<CellAudioClip> clips)
    {
        _fallOnGroundClips = clips;
        if (_groundLayerMask.value == 0)
            _groundLayerMask = 1 << LayerMask.NameToLayer("Environment");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_hasPlayedFallSound && ((_groundLayerMask & (1 << collision.gameObject.layer)) != 0))
        {
            PlayRandom(_grenadeAudioSource, _fallOnGroundClips);
            _hasPlayedFallSound = true;
        }
    }

    private void PlayRandom(AudioSource source, List<CellAudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        clips[Random.Range(0, clips.Count)].PlayAudioClipOneShot(source);
    }
}
