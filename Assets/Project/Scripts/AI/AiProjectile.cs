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

    [Header("Additional Settings")]
    public bool useAutoScaling = true;
    public float scaleMultipler = 45;

    public Vector3 direction { get; set; }
    public bool isActive { get; set; } = true;

    private Vector3 velocity;
    private TrailRenderer trail;
    private Rigidbody rb;

    private float _lifeTime = 5;

    public void Setup(Vector3 direction, float lifeTime, float speed)
    {
        if(trail == null)
            trail = GetComponentInChildren<TrailRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        this.direction = direction;
        this.speed = speed;
        _lifeTime = lifeTime;

        velocity = (direction) * (speed);

        rb.isKinematic = false;

        if (isActive)
            rb.AddForce(velocity, ForceMode.VelocityChange);

        transform.localScale = useAutoScaling ? Vector3.zero : Vector3.one * scaleMultipler;

        if (trail) trail.widthMultiplier = useAutoScaling ? 0 : scaleMultipler;
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
            if (trail) trail.widthMultiplier = scale;
        }

        if (!useAutoScaling)
        {
            transform.localScale = Vector3.one * scaleMultipler;
        }

        if (_lifeTime <= 0)
        {
            _lifeTime = int.MaxValue;
            rb.isKinematic = true;
            PoolManager.Instance.SetObject(this);

            return;
        }

        _lifeTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravity, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
