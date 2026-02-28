using System;
using UnityEngine;

[Serializable]
public class CellAudioClip
{
    public string NameAudioClip;
    public AudioClip AudioClip;
    [Range(0f, 1f)]
    public float Volume = 1;

    public void PlayAudioClip(AudioSource audioSource, float stepVolume = -1, bool isOneShot = false)
    {
        float volume = stepVolume < 0 ? Volume : stepVolume;

        if (isOneShot)
        {
            audioSource.PlayOneShot(AudioClip, volume);
            return;
        }

        audioSource.clip = AudioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void PlayAudioClipOneShot(AudioSource audioSource,  float stepVolume = -1)
    {
        PlayAudioClip(audioSource, stepVolume, true);
    }
}
