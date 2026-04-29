using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthquakeEffect : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _effects;
    [SerializeField] private float _timeDuration = 11f;

    private void OnDestroy()
    {
        Earthquake.Instance.StartEarthquake -= StartEffect;
    }

    private void Start()
    {
        Earthquake.Instance.StartEarthquake += StartEffect;

        foreach (Transform child in transform)
            if (child.TryGetComponent(out ParticleSystem particleSystem))
                _effects.Add(particleSystem);
    }

    public void ActivateEffect()
    {
        StartEffect(_timeDuration);
    }

    private void StartEffect(float duration)
    {
        StopAllCoroutines();

        foreach (var effect in _effects)
            effect.Stop();

        StartCoroutine(EffectPlay(duration));
    }

    private IEnumerator EffectPlay(float timeDuration)
    {
        foreach (var effect in _effects)
            effect.Play();

        yield return new WaitForSeconds(timeDuration);

        foreach (var effect in _effects)
            effect.Stop();
    }
}
