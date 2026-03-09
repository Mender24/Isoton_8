using UnityEngine;
using Akila.FPSFramework;

public class RangedCombatModule : MonoBehaviour, IRangedCombat
{
    [Header("Config")]
    [SerializeField] private RangedEnemyConfig _config;

    [Header("References")]
    [SerializeField] private Transform _shotOrigin;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    public bool  CanShoot    => !IsReloading && _config != null;
    public bool  IsFiring    => _state != null && _state.IsFiring;
    public bool  IsReloading => _state != null && _state.IsReloading;
    public float AttackRange => _config != null ? _config.AttackRange : 0f;
    public float ReloadTime  => _config != null ? _config.ReloadTime  : 3f;

    private EnemyState         _state;
    private EnemyNavigation    _navigation;
    private IEnemyAnimator     _animator;
    private IEnemyAudio        _audio;
    private EnemyDebugger      _debugger;
    private GrenadeThrowModule _grenadeModule;
    private bool _isPaused;

    private void Awake()
    {
        _state         = GetComponent<EnemyState>();
        _navigation    = GetComponent<EnemyNavigation>();
        _animator      = GetComponent<IEnemyAnimator>();
        _audio         = GetComponent<IEnemyAudio>();
        _debugger      = GetComponent<EnemyDebugger>();
        _grenadeModule = GetComponent<GrenadeThrowModule>();
    }

    private void Start()
    {
        _state.OnIsReloadingChanged += ChangeMoving;
    }

    public void SetPaused(bool paused) => _isPaused = paused;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void StartFire()
    {
        if (!CanShoot) return;
        _state.IsFiring = true;
    }

    public void StopFire()
    {
        _state.IsFiring      = false;
        _state.IsReloading   = false;
        _state.ShootCooldown = 0f;
        _animator?.SetReloading(false, 0f);
    }

    public void Tick(float deltaTime)
    {
        if (_isPaused) return;

        if (_state.ShootCooldown > 0f)
        {
            _state.ShootCooldown -= deltaTime;

            if (_state.IsReloading && _state.ShootCooldown <= 0f)
            {
                _state.IsReloading = false;
                _animator?.SetReloading(false, 0f);
                if (_state.PlayerIsSeen)
                    _state.IsFiring = true;
            }

            return;
        }

        if (!_state.IsFiring) return;

        Fire();
    }

    private void Fire()
    {
        if (!_state.PlayerIsSeen)
        {
            StopFire();
            return;
        }

        _state.CurrentBullet++;
        _grenadeModule?.OnBulletFired();

        Vector3 target = _playerTransform.position;
        target.y += Random.Range(-_config.HeightSprayOffset, _config.HeightSprayOffset) + _config.YOffset;
        target.x += Random.Range(-_config.WidthSprayOffset,  _config.WidthSprayOffset)  + _config.XOffset;

        SpawnBullet(target);
        TryDealDamage(target);
        _audio?.PlayAttackSound();

        if (_state.CurrentBullet >= _config.MagazineSize)
        {
            _state.CurrentBullet = 0;
            _state.IsFiring      = false;
            _state.IsReloading   = true;
            _state.ShootCooldown = _config.ReloadTime;
            _animator?.SetReloading(true, _config.ReloadTime);
            _audio?.PlayReloadSound();
        }
        else
        {
            _state.ShootCooldown = _config.FireRate;
        }
    }

    private void SpawnBullet(Vector3 target)
    {
        if (_config.BulletPrefab == null || _shotOrigin == null) return;

        AiProjectile bullet = PoolManager.Instance.GetObject<AiProjectile>();
        if (bullet == null) return;

        bullet.transform.position = _shotOrigin.position;
        bullet.gameObject.SetActive(true);
        bullet.Setup(
            _shotOrigin.position != target ? (target - _shotOrigin.position).normalized : transform.forward,
            _config.BulletLifetime,
            _config.BulletSpeed
        );
    }

    private void TryDealDamage(Vector3 target)
    {
        if (_shotOrigin == null || _playerTransform == null) return;

        Vector3 origin = _shotOrigin.position + transform.forward * 0.1f;
        Vector3 dir = (target - origin).normalized;
        bool hit = false;

        if (Random.value <= _config.ChanceToHit)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit rayHit, _config.AttackRange, _obstacleLayer | _playerLayer))
            {
                if (rayHit.collider.TryGetComponent(out Damageable damageable))
                {
                    damageable.Damage(_config.Damage, gameObject);
                    hit = true;
                }
                _debugger?.SetLastShot(origin, rayHit.point, hit);
                return;
            }
        }

        _debugger?.SetLastShot(origin, target, hit);
    }

    private void ChangeMoving(bool isMoving)
    {
        if (isMoving)
            _navigation.Stop();
        else 
            _navigation.Resume();
    }
}