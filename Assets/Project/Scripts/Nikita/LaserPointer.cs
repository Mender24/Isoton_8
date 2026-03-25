using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserPointer : MonoBehaviour
{
    [Header("Настройки лазера")]
    public Material laserMaterial;
    public float maxDistance = 100f;
    public float laserWidth = 0.03f;

    private LineRenderer _line;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();

        _line.positionCount = 2;
        _line.startWidth = laserWidth;
        _line.endWidth = laserWidth;
        _line.useWorldSpace = true;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;

        if (laserMaterial != null)
            _line.material = laserMaterial;
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        _line.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            // Луч упёрся в стену или объект
            _line.SetPosition(1, hit.point);
        }
        else
        {
            // Ничего не попало — тянем на максимум
            _line.SetPosition(1, origin + direction * maxDistance);
        }
    }
}