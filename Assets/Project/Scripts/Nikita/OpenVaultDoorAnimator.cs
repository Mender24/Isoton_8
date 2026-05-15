using System.Collections;
using UnityEngine;

public class OpenVaultDoorAnimator : MonoBehaviour
{
    [SerializeField] private Animator bunkerDoorAnimator;
    [SerializeField] private Animator bunkerHandAnimator;

    [Header("Movement Sound")]
    [SerializeField] private AudioSource hummingAudioSource;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 1f;

    private bool isAnimating = false;

    public void OpenDoor()
    {
        bunkerDoorAnimator.SetTrigger("OpenDoor");
        bunkerHandAnimator.SetTrigger("OpenDoor");
        StartCoroutine(TrackAnimation());
    }

    private IEnumerator TrackAnimation()
    {
        isAnimating = true;

        yield return null;
        while (bunkerDoorAnimator.IsInTransition(0))
            yield return null;

        while (bunkerDoorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        isAnimating = false;
    }

    private void Update()
    {
        UpdateHummingAudio();
    }

    private void UpdateHummingAudio()
    {
        if (hummingAudioSource == null) return;

        float targetVolume = isAnimating ? 1f : 0f;
        float fadeSpeed = isAnimating ? 1f / fadeInTime : 1f / fadeOutTime;
        hummingAudioSource.volume = Mathf.MoveTowards(hummingAudioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        if (hummingAudioSource.volume > 0.001f && !hummingAudioSource.isPlaying)
            hummingAudioSource.Play();
        else if (hummingAudioSource.volume <= 0.001f && hummingAudioSource.isPlaying)
            hummingAudioSource.Stop();
    }
}
