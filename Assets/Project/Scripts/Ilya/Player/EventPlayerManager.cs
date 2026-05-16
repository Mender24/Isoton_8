using Akila.FPSFramework;
using UnityEngine;

public class EventPlayerManager : MonoBehaviour
{
    private Damageable _damageable;

    private void OnDestroy()
    {
        if (_damageable != null)
            _damageable.onDamage -= OnDamage;
    }

    private void Start()
    {
        _damageable = GetComponent<Damageable>();

        if (_damageable != null)
            _damageable.onDamage += OnDamage;
    }

    private void OnDamage()
    {
        Earthquake.Instance.ShakeCamera();
    }
}
