using UnityEngine;
using UnityEngine.UI;

public class DamageCellView : MonoBehaviour
{
    [SerializeField] private float _maxAlpha = 0.5f;
    [SerializeField] private float _startRotation;
    [SerializeField] private float _maxDeviation;

    private Image _damageView;

    private float _speedUp = 0.5f;
    private float _speedDown = 0.5f;
    private float _timeView = 1f;
    private float _time = 0;

    private void Start()
    {
        _damageView = GetComponent<Image>();
    }

    private void Update()
    {
        if (_time > 0 && _damageView.color.a < 1)
        {
            AddAlphaImage();
        }
        else if (_time <= 0 && _damageView.color.a > 0)
        {
            OddAlphaImage();
        }

        _time -= Time.deltaTime;
    }

    public bool CheckInRange(float angle)
    {
        if (_startRotation - _maxDeviation < 0 && _startRotation + _maxDeviation > 0)
            angle = Mathf.Abs(angle);
        else if (angle < 0)
            angle += 365;

        return angle >= _startRotation - _maxDeviation && angle <= _startRotation + _maxDeviation;
    }

    private void AddAlphaImage()
    {
        if (_damageView.color.a == _maxAlpha)
            return;

        Color color;

        color = _damageView.color;
        color.a = Mathf.Lerp(color.a, _maxAlpha, _speedUp * Time.deltaTime);

        if (color.a > _maxAlpha)
            color.a = _maxAlpha;


        _damageView.color = color;
    }

    private void OddAlphaImage()
    {
        if (_damageView.color.a == 0)
            return;

        Color color;

        color = _damageView.color;
        color.a = Mathf.Lerp(color.a, 0, _speedDown * Time.deltaTime);

        if (color.a < 0)
            color.a = 0;

        _damageView.color = color;
    }

    public void StartViewDirectionDamage(float speedUp, float speedDown)
    {
        _time = _timeView;
        _speedUp = speedUp;
        _speedDown = speedDown;
    }
}
