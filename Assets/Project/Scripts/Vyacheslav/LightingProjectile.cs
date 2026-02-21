using UnityEngine;

public class LightingProjectile : AiProjectile
{
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _period = 0.5f;
    private float _nextAttackTime;
    [SerializeField] private float _baseLifeTime =10f;
    [SerializeField] private bool _isTestSize;
    [SerializeField] private Transform _testRadiusMesh;
    [SerializeField] private Transform _effect;
    [SerializeField] private float _addItionalSizeValue = 0.1f;
    [SerializeField] private float _updateSizePeriod = 0.5f;
    [SerializeField] private float _maxRadius = 5f;
     private float _nextUpdateSizeTime;
     private float _currentSize;

    public float LifeTime => _baseLifeTime;

    public override void Setup(Vector3 direction, float lifeTime, float speed)
    {
        base.Setup(direction, lifeTime, speed);
        SetupSize();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateSize();
        TryDoDamage();

    }

    private void SetupSize()
    {
        _currentSize = hitRadius;
        _nextUpdateSizeTime = Time.time + _updateSizePeriod;
        UpdateSize();
    }

    private void UpdateSize()
    {
        if (Time.time> _nextUpdateSizeTime)
        {
            _nextUpdateSizeTime = Time.time + _updateSizePeriod;
            _currentSize += _addItionalSizeValue;
            _currentSize = Mathf.Clamp(_currentSize, 0, _maxRadius);
            Debug.LogError("Current size" + _currentSize);
        }
        _effect.transform.localScale = _currentSize * Vector3.one;
        _testRadiusMesh.transform.localScale = _currentSize * 2 * Vector3.one;
        _testRadiusMesh.gameObject.SetActive(_isTestSize);
    }

    private void TryDoDamage()
    {
        if (_nextAttackTime > Time.time)
        {
            return;
        }
        _nextAttackTime = Time.time + _period;
        var enemies = EnemyCounter.Instance.GetEnemyBySphere(_currentSize, transform.position);
        foreach (var enemy in enemies)
        {
            enemy.Damage(_damage, null);
        }
    }

}

