using Akila.FPSFramework;
using UnityEngine;

public class AiProjectile : MonoBehaviour
{
    [Header("Base Settings")]
    public LayerMask hittableLayers = -1;
    public Vector3Direction decalDirection = Vector3Direction.forward;
    public float speed = 50;
    public float gravity = 1;
    public float force = 10;
    public GameObject defaultDecal;
    public float hitRadius = 0.03f;

    [Header("Hit Effects")]
    public GameObject hitEffectPrefab;        // Particle System ��� ���������
    public AudioClip hitSound;                // ���� ���������
    [Range(0f, 1f)] public float hitVolume = 1f;

    [Header("Additional Settings")]
    public bool useAutoScaling = true;
    public float scaleMultipler = 45;

    public Vector3 direction { get; set; }
    public bool isActive { get; set; } = true;

    private Vector3 _velocity;
    private TrailRenderer _trail;
    private Rigidbody _rb;
    private float _lifeTime = 5;

    public void ClearTrail()
    {
        if (_trail == null)
            _trail = GetComponentInChildren<TrailRenderer>(true);
        if (_trail != null)
            _trail.Clear();
    }

    public virtual void Setup(Vector3 direction, float lifeTime, float speed)
    {
        if (_trail == null)
            _trail = GetComponentInChildren<TrailRenderer>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        _trail?.Clear();

        this.direction = direction;
        this.speed = speed;
        _lifeTime = lifeTime;

        _velocity = direction * speed;
        _rb.isKinematic = false;

        if (isActive)
            _rb.AddForce(_velocity, ForceMode.VelocityChange);

        transform.localScale = useAutoScaling ? Vector3.zero : Vector3.one * scaleMultipler;
        if (_trail) _trail.widthMultiplier = useAutoScaling ? 0 : scaleMultipler;
    }

    private void Update()
    {
        if (useAutoScaling)
        {
            float distanceFromMainCamera = 1;
            float scale = 1;

            Camera mainCamera = FPSFrameworkUtility.GetMainCamera();

            if (mainCamera != null)
            {
                distanceFromMainCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
                scale = (distanceFromMainCamera * scaleMultipler) * (mainCamera.fieldOfView / 360);
            }

            transform.localScale = Vector3.one * scale;
            if (_trail) _trail.widthMultiplier = scale;
        }
        else
        {
            transform.localScale = Vector3.one * scaleMultipler;
        }

        if (_lifeTime <= 0)
        {
            ReturnToPool();
            return;
        }

        _lifeTime -= Time.deltaTime;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // ��������� ����
        if ((hittableLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        ContactPoint contact = collision.GetContact(0);

        // ����
        ApplyDamage(collision, contact);

        // �������
        SpawnHitEffect(contact);
        SpawnDecal(collision, contact);
        PlayHitSound(contact);

        // ������
        if (collision.rigidbody != null)
            collision.rigidbody.AddForceAtPosition(direction * force, contact.point, ForceMode.Impulse);

        ReturnToPool();
    }

    protected virtual void ApplyDamage(Collision collision, ContactPoint contact)
    {
        // ������� ����� �� ����� ���� � ���������������� � ����������
        // ���� RangedCombatModule ��� ������� ���� ����� TryDealDamage
    }

    private void SpawnHitEffect(ContactPoint contact)
    {
        if (hitEffectPrefab == null) return;

        GameObject effect = Instantiate(
            hitEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        // ���� ��� ParticleSystem � ������������ ����� ������������
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(effect, 3f);
    }

    private void SpawnDecal(Collision collision, ContactPoint contact)
    {
        if (defaultDecal == null) return;

        Quaternion decalRotation = Quaternion.LookRotation(-contact.normal);
        GameObject decal = Instantiate(defaultDecal, contact.point, decalRotation);
        decal.transform.SetParent(collision.transform);
        Destroy(decal, 60f);
    }

    private void PlayHitSound(ContactPoint contact)
    {
        if (hitSound == null) return;
        AudioSource.PlayClipAtPoint(hitSound, contact.point, hitVolume);
    }

    protected void ReturnToPool()
    {
        _lifeTime = int.MaxValue;
        _rb.isKinematic = true;
        if (_trail != null) _trail.Clear();
        PoolManager.Instance.SetObject(this);
    }

    protected virtual void FixedUpdate()
    {
        _rb.AddForce(Physics.gravity * gravity, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Camera.current.tag == "MainCamera" || Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
#endif
    }
}