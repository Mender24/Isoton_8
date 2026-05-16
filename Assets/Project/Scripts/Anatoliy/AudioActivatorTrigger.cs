using UnityEngine;
using Akila.FPSFramework;

public class AudioActivatorTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    void Start()
    {
        if (_audioClip == null || _audioSource == null)
            Debug.LogError("No audio source or audio clip was assigned at " + gameObject.name);
    }

    public void PlayOneShotSound()
    {
        if (_audioSource == null || _audioClip == null) return;

        _audioSource.PlayOneShot(_audioClip);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player _))
            PlayOneShotSound();
    }
}
