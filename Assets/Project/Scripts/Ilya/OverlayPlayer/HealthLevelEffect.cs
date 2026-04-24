using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.UI;

public class HealthLevelEffect : MonoBehaviour
{
    [SerializeField] private float _startHealthPercentage = 0.7f;
    [SerializeField] private float _endHealthPercentage = 0.4f;
    [Space]
    [Header("Alpha")]
    [SerializeField] private float _speedUp = 0.5f;
    [SerializeField] private float _speedDown = 2f;
    [SerializeField] private float _startAlphaPercentage = 0.1f;
    [SerializeField] private float _endAlphaPercentage = 1f;

    private IDamageable _damageable;
    private float _maxHealth = 0;

    private Image _imageView;

    private bool _isDirectionUp = false;
    private float _currentAlpha = 0;
    private float _targetAlpha = 0;

    private void Start()
    {
        _damageable = Player.Instance.Actor.Damageable;
        _maxHealth = _damageable.Health;

        if (_imageView == null)
            _imageView = GetComponent<Image>();
    }

    private void Update()
    {
        CheckBorder();
        ChangeAlpha();
    }

    private void ChangeAlpha()
    {
        if (_imageView == null)
            return;

        Color newColor = _imageView.color;
        _currentAlpha = newColor.a;

        float t = 1 - Mathf.Exp(-(_isDirectionUp ? _speedUp : _speedDown) * Time.deltaTime); 
        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
        newColor.a = _currentAlpha;

        _imageView.color = newColor;
    }

    private void CheckBorder()
    {
        _imageView.enabled = _damageable.Health / _maxHealth < _startHealthPercentage;

        if (_damageable.Health / _maxHealth < _startHealthPercentage)
            ChangeBorderEffect();
    }

    private void ChangeBorderEffect()
    {
        float currentPercentage = _damageable.Health / _maxHealth;
        float newTargetAlpha = (_startHealthPercentage - currentPercentage) / (_startHealthPercentage - _endHealthPercentage);

        if(newTargetAlpha < _startAlphaPercentage)
            newTargetAlpha = _startAlphaPercentage;

        if(newTargetAlpha > _endAlphaPercentage)
            newTargetAlpha = _endAlphaPercentage;

        if (_targetAlpha < _currentAlpha)
            _isDirectionUp = false;
        else
            _isDirectionUp = true;

        _targetAlpha = newTargetAlpha;
    }
}
