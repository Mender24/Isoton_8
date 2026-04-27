using UnityEngine;
using System.Collections.Generic;

public class BallLightningController : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private float outerRadius = 8f;
    [SerializeField] private float innerRadius = 2f;
    [SerializeField] private float zoneHeight = 4f;

    [Header("Movement")]
    [SerializeField] private float minOrbitSpeed = 1f;
    [SerializeField] private float maxOrbitSpeed = 3f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float verticalFloatSpeed = 0.3f;
    [SerializeField] private float smoothTime = 2f;

    [Header("Random Drift")]
    [SerializeField] private float randomDriftStrength = 0.3f;
    [SerializeField] private float directionChangeChance = 0.3f;

    [Header("Visual")]
    [SerializeField] private float microMovement = 0.05f;
    [SerializeField] private float microMovementSpeed = 3f;

    private List<Ball> balls = new List<Ball>();

    private class Ball
    {
        public Transform transform;

        public float currentAngle;
        public float currentRadius;
        public float currentHeight;

        public float orbitSpeed;        // Фиксированная скорость (не меняется)
        public int orbitDirection;      // 1 или -1
        public float radiusVelocity;
        public float heightVelocity;

        public float targetRadius;
        public float targetHeight;

        public float perlinSeedX;
        public float perlinSeedY;
        public float perlinSeedZ;

        public float targetChangeTimer;
        public float directionChangeTimer;
    }

    void Start()
    {
        InitializeBalls();
    }

    void InitializeBalls()
    {
        int ballCount = transform.childCount;

        for (int i = 0; i < ballCount; i++)
        {
            Transform child = transform.GetChild(i);

            Ball ball = new Ball();
            ball.transform = child;

            // Начальная позиция
            ball.currentAngle = Random.Range(0f, 360f);
            ball.currentRadius = Random.Range(innerRadius + 0.5f, outerRadius - 0.5f);
            ball.currentHeight = Random.Range(-zoneHeight / 2f + 0.5f, zoneHeight / 2f - 0.5f);

            // Равномерно распределяем по направлениям
            if (i < ballCount / 2)
            {
                ball.orbitDirection = 1;  // По часовой
            }
            else
            {
                ball.orbitDirection = -1; // Против часовой
            }

            // Случайная скорость в заданном диапазоне
            ball.orbitSpeed = Random.Range(minOrbitSpeed, maxOrbitSpeed);

            ball.radiusVelocity = 0f;
            ball.heightVelocity = 0f;

            ball.perlinSeedX = Random.Range(0f, 100f);
            ball.perlinSeedY = Random.Range(0f, 100f);
            ball.perlinSeedZ = Random.Range(0f, 100f);

            SetNewRadiusTarget(ball);
            SetNewHeightTarget(ball);

            ball.targetChangeTimer = Random.Range(0f, 3f);
            ball.directionChangeTimer = Random.Range(5f, 10f);

            balls.Add(ball);
            UpdateBallPosition(ball);
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (Ball ball in balls)
        {
            UpdateBallTimers(ball);
            UpdateBallMovement(ball, deltaTime);
            ApplyMicroMovement(ball, deltaTime);
            ClampBallPosition(ball);
        }
    }

    void UpdateBallTimers(Ball ball)
    {
        // Таймер смены радиуса и высоты
        ball.targetChangeTimer -= Time.deltaTime;
        if (ball.targetChangeTimer <= 0f)
        {
            SetNewRadiusTarget(ball);
            SetNewHeightTarget(ball);
            ball.targetChangeTimer = Random.Range(3f, 7f);
        }

        // Таймер возможной смены направления
        ball.directionChangeTimer -= Time.deltaTime;
        if (ball.directionChangeTimer <= 0f)
        {
            if (Random.value < directionChangeChance)
            {
                ball.orbitDirection *= -1;
            }
            ball.directionChangeTimer = Random.Range(8f, 15f);
        }
    }

    void SetNewRadiusTarget(Ball ball)
    {
        ball.targetRadius = Random.Range(innerRadius + 0.5f, outerRadius - 0.5f);
    }

    void SetNewHeightTarget(Ball ball)
    {
        ball.targetHeight = Random.Range(-zoneHeight / 2f + 0.5f, zoneHeight / 2f - 0.5f);
    }

    void UpdateBallMovement(Ball ball, float deltaTime)
    {
        // Постоянная скорость вращения (не ускоряется)
        ball.currentAngle += ball.orbitSpeed * ball.orbitDirection * deltaTime;

        // Плавное движение к целевому радиусу
        float radiusDiff = ball.targetRadius - ball.currentRadius;
        ball.radiusVelocity = Mathf.Lerp(ball.radiusVelocity, radiusDiff * floatSpeed, deltaTime * 0.5f);

        // Плавное движение к целевой высоте
        float heightDiff = ball.targetHeight - ball.currentHeight;
        ball.heightVelocity = Mathf.Lerp(ball.heightVelocity, heightDiff * verticalFloatSpeed, deltaTime * 0.5f);

        // Добавление плавного случайного дрейфа через Perlin noise
        float noiseRadius = (Mathf.PerlinNoise(Time.time * 0.2f, ball.perlinSeedX) - 0.5f) * 2f;
        float noiseHeight = (Mathf.PerlinNoise(Time.time * 0.2f, ball.perlinSeedY) - 0.5f) * 2f;

        // Применение движения с ограничением скорости
        float maxRadiusSpeed = floatSpeed * 2f;
        float maxHeightSpeed = verticalFloatSpeed * 2f;

        ball.radiusVelocity += noiseRadius * randomDriftStrength * deltaTime;
        ball.heightVelocity += noiseHeight * randomDriftStrength * deltaTime;

        // Ограничение максимальной скорости
        ball.radiusVelocity = Mathf.Clamp(ball.radiusVelocity, -maxRadiusSpeed, maxRadiusSpeed);
        ball.heightVelocity = Mathf.Clamp(ball.heightVelocity, -maxHeightSpeed, maxHeightSpeed);

        // Применение скоростей
        ball.currentRadius += ball.radiusVelocity * deltaTime;
        ball.currentHeight += ball.heightVelocity * deltaTime;

        // Плавное ограничение у границ
        SmoothBoundaryClamp(ball);

        // Нормализация угла
        ball.currentAngle = ball.currentAngle % 360f;
        if (ball.currentAngle < 0f) ball.currentAngle += 360f;

        // Обновление позиции
        UpdateBallPosition(ball);
    }

    void SmoothBoundaryClamp(Ball ball)
    {
        float margin = 0.5f;

        // Внутренняя граница
        if (ball.currentRadius < innerRadius + margin)
        {
            float t = Mathf.InverseLerp(innerRadius, innerRadius + margin, ball.currentRadius);
            ball.currentRadius = Mathf.Lerp(innerRadius + margin, ball.currentRadius, t * t);
            ball.radiusVelocity = Mathf.Max(ball.radiusVelocity, 0f);
        }

        // Внешняя граница
        if (ball.currentRadius > outerRadius - margin)
        {
            float t = Mathf.InverseLerp(outerRadius, outerRadius - margin, ball.currentRadius);
            ball.currentRadius = Mathf.Lerp(outerRadius - margin, ball.currentRadius, t * t);
            ball.radiusVelocity = Mathf.Min(ball.radiusVelocity, 0f);
        }

        // Границы по высоте
        float heightMargin = margin * 0.5f;
        if (Mathf.Abs(ball.currentHeight) > zoneHeight / 2f - heightMargin)
        {
            float maxHeight = zoneHeight / 2f - heightMargin;
            float sign = Mathf.Sign(ball.currentHeight);

            float t = Mathf.InverseLerp(maxHeight + heightMargin, maxHeight, Mathf.Abs(ball.currentHeight));
            ball.currentHeight = Mathf.Lerp(sign * maxHeight, ball.currentHeight, t * t);
            ball.heightVelocity = -sign * Mathf.Abs(ball.heightVelocity);
        }
    }

    void ApplyMicroMovement(Ball ball, float deltaTime)
    {
        Vector3 microOffset = new Vector3(
            Mathf.PerlinNoise(Time.time * microMovementSpeed * 1.3f, ball.perlinSeedX) - 0.5f,
            Mathf.PerlinNoise(Time.time * microMovementSpeed * 1.7f, ball.perlinSeedY) - 0.5f,
            Mathf.PerlinNoise(Time.time * microMovementSpeed * 1.5f, ball.perlinSeedZ) - 0.5f
        ) * microMovement;

        ball.transform.position += microOffset * deltaTime;
    }

    void ClampBallPosition(Ball ball)
    {
        Vector3 relativePos = ball.transform.position - transform.position;
        Vector3 horizontalPos = new Vector3(relativePos.x, 0f, relativePos.z);

        float distance = horizontalPos.magnitude;

        // Жесткая проверка на всякий случай
        if (distance > outerRadius)
        {
            horizontalPos = horizontalPos.normalized * (outerRadius - 0.1f);
            ball.currentRadius = outerRadius - 0.1f;
            ball.radiusVelocity = -Mathf.Abs(ball.radiusVelocity);
        }
        else if (distance < innerRadius)
        {
            if (distance > 0.01f)
            {
                horizontalPos = horizontalPos.normalized * (innerRadius + 0.1f);
            }
            else
            {
                horizontalPos = Random.insideUnitCircle.normalized * (innerRadius + 0.1f);
            }
            ball.currentRadius = innerRadius + 0.1f;
            ball.radiusVelocity = Mathf.Abs(ball.radiusVelocity);
        }

        float clampedHeight = Mathf.Clamp(relativePos.y, -zoneHeight / 2f + 0.1f, zoneHeight / 2f - 0.1f);

        ball.transform.position = new Vector3(
            transform.position.x + horizontalPos.x,
            transform.position.y + clampedHeight,
            transform.position.z + horizontalPos.z
        );

        ball.currentHeight = clampedHeight;
    }

    void UpdateBallPosition(Ball ball)
    {
        float angleRad = ball.currentAngle * Mathf.Deg2Rad;

        Vector3 targetPosition = new Vector3(
            transform.position.x + Mathf.Cos(angleRad) * ball.currentRadius,
            transform.position.y + ball.currentHeight,
            transform.position.z + Mathf.Sin(angleRad) * ball.currentRadius
        );

        // Плавное перемещение к целевой позиции
        ball.transform.position = Vector3.Lerp(
            ball.transform.position,
            targetPosition,
            Time.deltaTime * 15f
        );
    }

    void OnDrawGizmosSelected()
    {
        // Внутренняя зона
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        DrawCylinderGizmo(innerRadius, zoneHeight);

        // Внешняя зона
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.15f);
        DrawCylinderGizmo(outerRadius, zoneHeight);

        // Средняя траектория
        float midRadius = (innerRadius + outerRadius) / 2f;
        Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.2f);
        DrawCylinderGizmo(midRadius, zoneHeight);
    }

    void DrawCylinderGizmo(float radius, float height)
    {
        Vector3 center = transform.position;
        Vector3 topCenter = center + Vector3.up * height / 2f;
        Vector3 bottomCenter = center - Vector3.up * height / 2f;

        int segments = 48;
        Vector3 prevTop = topCenter + new Vector3(radius, 0f, 0f);
        Vector3 prevBottom = bottomCenter + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            Vector3 topPoint = topCenter + offset;
            Vector3 bottomPoint = bottomCenter + offset;

            Gizmos.DrawLine(prevTop, topPoint);
            Gizmos.DrawLine(prevBottom, bottomPoint);
            Gizmos.DrawLine(topPoint, bottomPoint);

            prevTop = topPoint;
            prevBottom = bottomPoint;
        }
    }
}