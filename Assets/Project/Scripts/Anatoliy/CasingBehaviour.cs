using UnityEngine;

public class CasingBehaviour : MonoBehaviour
{
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float pitchMin = 0.85f;
    [SerializeField] private float pitchMax = 1.15f;
    [SerializeField] private float minImpactVelocity = 0.5f;

    private bool _hasPlayed;

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasPlayed) return;
        if (hitSounds == null || hitSounds.Length == 0) return;
        if (collision.relativeVelocity.magnitude < minImpactVelocity) return;

        _hasPlayed = true;

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        float pitch = Random.Range(pitchMin, pitchMax);

        GameObject tempAudio = new GameObject("CasingSound");
        tempAudio.transform.position = collision.contacts[0].point;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = 1f;
        source.Play();

        Destroy(tempAudio, clip.length / pitch);
    }
}
