using UnityEngine;

public enum GrenadeThrowPhase { Idle, WindingUp, Throwing }

public class GrenadeThrowModule : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private EnemyGrenadeConfig _config;

    [Header("References")]
    [SerializeField] private Transform _throwOrigin;
    [SerializeField] private Transform _grenadeHandBone;

    private EnemyState         _state;
    private IEnemyAnimator     _animator;
    private BasicEnemyAnimator _basicAnimator;
    private Transform          _playerTransform;

    private int _bulletsFiredSinceCheck;
    private GameObject _heldGrenade;

    public GrenadeThrowPhase Phase { get; private set; } = GrenadeThrowPhase.Idle;

    public bool CanThrowGrenade => _config != null
                                && _state.GrenadeCooldown <= 0f
                                && Phase == GrenadeThrowPhase.Idle;

    private void Awake()
    {
        _state         = GetComponent<EnemyState>();
        _animator      = GetComponent<IEnemyAnimator>();
        _basicAnimator = GetComponent<BasicEnemyAnimator>();
    }

    private void Start()
    {
        if (_basicAnimator != null)
        {
            _basicAnimator.OnGrenadeWindUpComplete += HandleWindUpComplete;
            _basicAnimator.OnGrenadeReleasePoint   += HandleReleasePoint;
            _basicAnimator.OnGrenadeThrowComplete  += HandleThrowComplete;
        }
    }

    private void OnDestroy()
    {
        if (_basicAnimator != null)
        {
            _basicAnimator.OnGrenadeWindUpComplete -= HandleWindUpComplete;
            _basicAnimator.OnGrenadeReleasePoint   -= HandleReleasePoint;
            _basicAnimator.OnGrenadeThrowComplete  -= HandleThrowComplete;
        }
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void OnBulletFired()
    {
        if (_config == null || _state.IsThrowingGrenade) return;

        _bulletsFiredSinceCheck++;
        if (_bulletsFiredSinceCheck >= _config.BulletsBeforeCheck)
        {
            _bulletsFiredSinceCheck = 0;
            if (CanThrowGrenade && Random.value <= _config.ThrowChance)
                _state.ShouldThrowGrenade = true;
        }
    }

    public void StartWindUp()
    {
        if (!CanThrowGrenade) return;

        Phase = GrenadeThrowPhase.WindingUp;
        _state.IsThrowingGrenade = true;
        SpawnHeldGrenade();
        _animator?.TriggerGrenadeWindUp(_config.WindUpDuration);
    }

    private void HandleWindUpComplete()
    {
        if (Phase != GrenadeThrowPhase.WindingUp) return;

        Phase = GrenadeThrowPhase.Throwing;
        _animator?.TriggerGrenadeThrow(_config.ThrowDuration);
    }

    private void HandleReleasePoint()
    {
        if (Phase != GrenadeThrowPhase.Throwing) return;
        DestroyHeldGrenade();
        SpawnGrenade();
    }

    private void HandleThrowComplete()
    {
        if (Phase != GrenadeThrowPhase.Throwing) return;
        FinishThrow();
    }

    public void Cancel()
    {
        if (Phase == GrenadeThrowPhase.Idle) return;

        Phase = GrenadeThrowPhase.Idle;
        _state.IsThrowingGrenade  = false;
        _state.ShouldThrowGrenade = false;
        DestroyHeldGrenade();
        _animator?.CancelGrenadeThrow();
    }

    public void Tick(float deltaTime)
    {
        if (_state.GrenadeCooldown > 0f)
            _state.GrenadeCooldown -= deltaTime;
    }

    private void FinishThrow()
    {
        Phase = GrenadeThrowPhase.Idle;
        _state.IsThrowingGrenade  = false;
        _state.ShouldThrowGrenade = false;
        _state.GrenadeCooldown    = _config.ThrowCooldown;
    }

    private void SpawnHeldGrenade()
    {
        GameObject prefab = _config.GrenadeInHandPrefab != null
            ? _config.GrenadeInHandPrefab
            : _config.GrenadePrefab;

        if (prefab == null) return;

        Transform parent = _grenadeHandBone != null ? _grenadeHandBone : _throwOrigin;
        if (parent == null) parent = transform;

        _heldGrenade = Instantiate(prefab, parent.position, parent.rotation, parent);

        if (_heldGrenade.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
        foreach (var col in _heldGrenade.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    private void DestroyHeldGrenade()
    {
        if (_heldGrenade != null)
        {
            Destroy(_heldGrenade);
            _heldGrenade = null;
        }
    }

    private void SpawnGrenade()
    {
        if (_config?.GrenadePrefab == null || _playerTransform == null) return;

        Vector3 origin = _throwOrigin != null
            ? _throwOrigin.position
            : transform.position + Vector3.up * 1.5f;

        Vector2 randomCircle = Random.insideUnitCircle * _config.LandingRandomRadius;
        Vector3 landing = _playerTransform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        GameObject grenade = Instantiate(_config.GrenadePrefab, origin, Quaternion.identity);

        if (grenade.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = CalculateThrowVelocity(origin, landing, _config.ThrowTime);
    }

    private static Vector3 CalculateThrowVelocity(Vector3 from, Vector3 to, float time)
    {
        // v = (d - 0.5 * g * t^2) / t
        return (to - from) / time - 0.5f * Physics.gravity * time;
    }
}
