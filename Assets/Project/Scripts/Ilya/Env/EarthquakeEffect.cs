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

    private void StartEffect()
    {
        foreach (var effect in _effects)
            effect.Play();
    }
}
