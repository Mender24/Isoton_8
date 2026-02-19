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

    private Vector3 _velocity;
    private TrailRenderer _trail;
    private Rigidbody _rb;
    private float _lifeTime = 5;

    public virtual void Setup(Vector3 direction, float lifeTime, float speed)
    {
        if(_trail == null)
            _trail = GetComponentInChildren<TrailRenderer>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody>();

        this.direction = direction;
        this.speed = speed;
        _lifeTime = lifeTime;

        _velocity = (direction) * (speed);
        _rb.isKinematic = false;

        if (isActive)
            _rb.AddForce(_velocity, ForceMode.VelocityChange);

        transform.localScale = useAutoScaling ? Vector3.one : Vector3.one * scaleMultipler;

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

        if (!useAutoScaling)
        {
            transform.localScale = Vector3.one * scaleMultipler;
        }

        if (_lifeTime <= 0)
        {
            _lifeTime = int.MaxValue;
            _rb.isKinematic = true;
            PoolManager.Instance.SetObject(this);
            Debug.LogError("StopProj");
            return;
        }

        _lifeTime -= Time.deltaTime;
    }

    protected virtual void FixedUpdate()
    {
        _rb.AddForce(Physics.gravity * gravity, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    { 
#if UNITY_EDITOR
        // To make only main camera and scene view draw gizmos
        if (Camera.current.tag == "MainCamera" || Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
#endif
    }
}
