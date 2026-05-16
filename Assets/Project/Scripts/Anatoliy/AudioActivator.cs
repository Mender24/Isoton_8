using System.Collections;
using UnityEngine;

public class AudioActivator : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private float _cutFromEndInSec = 0f;
    [SerializeField] private float _cutFromStartInSec = 0f;

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

    public void PlaySound()
    {
        if (_audioSource == null || _audioSource == null) return;

        _audioSource.clip = _audioClip;
        _audioSource.Play();
    }

    public void StopSound()
    {
        if (_audioSource == null) return;

        _audioSource.Stop();
    }

    public void PlayTrimmedSound()
    {
        if (_audioSource == null || _audioClip == null) return;

        float duration = _audioClip.length - _cutFromStartInSec - _cutFromEndInSec;
        if (duration <= 0f) return;

        _audioSource.clip = _audioClip;
        _audioSource.time = _cutFromStartInSec;
        _audioSource.Play();

        StartCoroutine(StopAfter(duration));
    }

    private IEnumerator StopAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _audioSource.Stop();
    }
}
