using UnityEngine;

public class LaserLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform laserStartPoint;

    [Header("Отладка")]
    [SerializeField] private bool showDebugRay = true;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer не найден на объекте " + gameObject.name);
            return;
        }

        // Линия будет состоять из 2 точек
        lineRenderer.positionCount = 2;

        // ВАЖНО: убедитесь, что линия рисуется в МИРОВЫХ координатах
        // В инспекторе LineRenderer должно быть: Space = World
    }

    void Update()
    {
        if (lineRenderer == null) return;

        UpdateLaser();
    }

    private void UpdateLaser()
    {
        // Определяем точку старта
        Vector3 startPoint;

        if (laserStartPoint != null)
        {
            startPoint = laserStartPoint.position;
        }
        else
        {
            startPoint = transform.position;
        }

        // Направление - всегда мировое направление объекта
        Vector3 direction = transform.forward;

        // Для отладки - визуализируем луч в Scene View
        if (showDebugRay)
        {
            Debug.DrawRay(startPoint, direction * maxDistance, Color.red);
        }

        // Пускаем луч
        if (Physics.Raycast(startPoint, direction, out RaycastHit hit, maxDistance, obstacleLayer))
        {
            // Луч попал в препятствие
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, hit.point);

            // Для отладки - отмечаем точку попадания
            if (showDebugRay)
            {
                Debug.DrawLine(startPoint, hit.point, Color.green);
            }
        }
        else
        {
            // Луч не попал
            Vector3 endPoint = startPoint + direction * maxDistance;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && lineRenderer != null)
        {
            // Показываем в редакторе, где будет лазер
            Vector3 startPoint = laserStartPoint != null ? laserStartPoint.position : transform.position;
            Vector3 direction = transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint, 0.1f);
            Gizmos.DrawLine(startPoint, startPoint + direction * maxDistance);
        }
    }
}