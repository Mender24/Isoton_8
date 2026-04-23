using UnityEngine;
using Beautify.Universal;
using Akila.FPSFramework;

public class HealthVignette : MonoBehaviour
{
    [Header("OuterRing")]
    [SerializeField] private float _baseOuterRing = 0.4f;
    [SerializeField] private float _newOuterRing = 0.76f;
    [Header("InnerRing")]
    [SerializeField] private float _baseInnerRing = 0.2f;
    [SerializeField] private float _newInnerRing = 0.92f;
    [Header("Fade")]
    [SerializeField] private float _baseFade = 0;
    [SerializeField] private float _newFade = 0.37f;
    [Space]
    [SerializeField] private float _speedChangeUp = 1f;
    [SerializeField] private float _speedChangeDown = 1f;
    [Space]
    [Header("BorderHealth")]
    [SerializeField] private float _startPercentageHealth = 0.4f;

    private IDamageable _damageable;
    private float _maxHealth;

    private bool _isActive = false;

    private void Start()
    {
        if (Player.Instance == null)
            return;

        _damageable = Player.Instance.Actor.Damageable;
        _maxHealth = _damageable.Health;
    }

    private void Update()
    {
        ChangeActive();
        ChangeVignette();
    }

    private void ChangeActive()
    {
        if (_damageable.Health / _maxHealth < _startPercentageHealth)
            _isActive = true;
        else
            _isActive = false;
    }

    private void ChangeVignette()
    {
        float t = 1 - Mathf.Exp(-(_isActive ? _speedChangeUp : _speedChangeDown) * Time.deltaTime);
        float value = BeautifySettings.sharedSettings.vignettingOuterRing.value;
        float newOuterRing = Mathf.Lerp(value, _isActive ? _newOuterRing : _baseOuterRing, t);
        value = BeautifySettings.sharedSettings.vignettingInnerRing.value;
        float newInnerRing = Mathf.Lerp(value, _isActive ? _newInnerRing : _baseInnerRing, t);
        value = BeautifySettings.sharedSettings.vignettingFade.value;
        float newFade = Mathf.Lerp(value, _isActive ? _newFade : _baseFade, t);

        BeautifySettings.sharedSettings.vignettingOuterRing.value = newOuterRing;
        BeautifySettings.sharedSettings.vignettingInnerRing.value = newInnerRing;
        BeautifySettings.sharedSettings.vignettingFade.value = newFade;
    }
}
