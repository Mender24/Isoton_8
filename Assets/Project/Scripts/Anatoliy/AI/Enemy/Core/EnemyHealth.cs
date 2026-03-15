using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Akila.FPSFramework;
using Biostart.Impact;
using Biostart.Blood;


/// <summary>
/// Отвечает за здоровье, получение урона, смерть и ragdoll.
/// Реализует IDamageable. При смерти стреляет OnDeath.
/// </summary>
[RequireComponent(typeof(EnemyState))]
public class EnemyHealth : MonoBehaviour, IDamageable, IOnHitInChildren
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;

    [Header("Death")]
    [SerializeField] private Ragdoll _ragdoll;
    [SerializeField] private float _disableCollidersDelay = 3f;
    [SerializeField] private float _deactivateSelfDelay = 20f;

    [Header("Damage Reaction")]
    [SerializeField] private bool _enableReactionCooldown = true;
    [SerializeField] private float _reactionCooldown = 3f;

    public UnityEvent OnDeathInternal = new();
    public UnityEvent<float, GameObject> OnDamaged = new(); 

    public float Health { get => _health; set => _health = value; }
    public bool isDamagableDisabled { get; set; }
    public bool allowDamageableEffects { get; set; }
    public bool DeadConfirmed { get; set; }
    public GameObject DamageSource { get; set; }

    UnityEvent IDamageable.OnDeath => OnDeathInternal;

    [SerializeField] private CapsuleCollider _capsuleCollider;

    private float _health;
    private float _lastReactionTime = -999f;
    private EnemyState _state;
    private IEnemyAnimator _animator;
    private IEnemyAudio _audio;
    private ImpactEffect _impactEffect;
    private BloodProjector _bloodProjector;

    private void Awake()
    {
        _state         = GetComponent<EnemyState>();
        _animator      = GetComponent<IEnemyAnimator>();
        _audio         = GetComponent<IEnemyAudio>();
        _impactEffect   = GetComponent<ImpactEffect>();
        _bloodProjector = GetComponent<BloodProjector>();
        _health        = _maxHealth;
    }

    public void Damage(float amount, GameObject damageSource)
    {
        if (_state.IsDead || isDamagableDisabled) return;

        _health -= amount;
        OnDamaged?.Invoke(amount, damageSource);

        TryPlayHitReaction();

        if (_health <= 0f)
            Die();
    }

    public void OnHitInChildren(HitInfo hitInfo)
    {
        if (_state.IsDead) return;

        RaycastHit hit = hitInfo.raycastHit;

        if (_impactEffect != null)
            _impactEffect.SpawnBloodEffect(hit.point, hit.normal);

        if (_bloodProjector != null && _bloodProjector.bloodProjectorPrefab != null)
        {
            Vector3 pos = hit.point + hit.normal * _bloodProjector.zOffset;
            GameObject proj = Instantiate(_bloodProjector.bloodProjectorPrefab, pos, Quaternion.LookRotation(hit.normal));
            proj.transform.SetParent(hit.collider != null ? hit.collider.transform : transform);
            Destroy(proj, _bloodProjector.destroyTime);
        }
    }

    private void TryPlayHitReaction()
    {
        if (!_enableReactionCooldown ||
            Time.time - _lastReactionTime >= _reactionCooldown)
        {
            _lastReactionTime = Time.time;
            _animator?.PlayHit();
            _audio?.PlayHitSound();
        }
    }

    private void Die()
    {
        _state.IsDead = true;
        _state.IsActivated = false;
        _state.PlayerDetected = false;
        _state.IsFiring = false;
        _state.IsAlerted = false;
        _state.IsSearching = false;
        _state.IsMeleeAttacking = false;

        OnDeathInternal?.Invoke();

        _audio?.PlayDeathSound();
        _animator?.SetDead(true);
        _ragdoll?.Enable();

        StartCoroutine(DisableCollidersRoutine());
        StartCoroutine(DeactivateSelfRoutine());
    }

    private IEnumerator DisableCollidersRoutine()
    {
        yield return new WaitForSeconds(_disableCollidersDelay);

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;
    }

    private IEnumerator DeactivateSelfRoutine()
    {
        yield return new WaitForSeconds(_deactivateSelfDelay);
        gameObject.SetActive(false);
    }

    public bool IsSwaped() => true;

    public void Register() { }

    public bool IsSphereCollision(Vector3 sphereCenter, float sphereRadius)
    {
        if (_capsuleCollider == null) return false;

        float halfHeight = _capsuleCollider.height * 0.5f;
        Vector3 enemyCenter = transform.position + Vector3.up * halfHeight;

        return Mathf.Abs(enemyCenter.x - sphereCenter.x) < sphereRadius + _capsuleCollider.radius
            && Mathf.Abs(enemyCenter.z - sphereCenter.z) < sphereRadius + _capsuleCollider.radius
            && Mathf.Abs(enemyCenter.y - sphereCenter.y) < sphereRadius + halfHeight;
    }

    public void ResetHealth()
    {
        _health = _maxHealth;
        _lastReactionTime = -999f;
        DeadConfirmed = false;

        _ragdoll?.Disable();

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = true;

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;
    }
}