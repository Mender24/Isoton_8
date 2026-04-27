using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float minRotationSpeed = 30f;
    [SerializeField] private float maxRotationSpeed = 120f;
    [SerializeField] private float directionChangeInterval = 3f;
    [SerializeField] private float smoothTime = 0.5f;

    [Header("Wobble")]
    [SerializeField] private bool enableWobble = true;
    [SerializeField] private float wobbleIntensity = 0.3f;
    [SerializeField] private float wobbleFrequency = 1.5f;

    private Vector3 currentRotationAxis;
    private float currentRotationSpeed;
    private Vector3 targetRotationAxis;
    private float targetRotationSpeed;
    private float directionTimer;
    private float perlinSeed;

    void Start()
    {
        // Случайное начальное вращение
        transform.rotation = Random.rotation;

        // Генерация первой цели
        GenerateNewTarget();
        currentRotationAxis = targetRotationAxis;
        currentRotationSpeed = targetRotationSpeed;

        perlinSeed = Random.Range(0f, 100f);
        directionTimer = Random.Range(0f, directionChangeInterval);
    }

    void Update()
    {
        // Таймер смены направления вращения
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            GenerateNewTarget();
            directionTimer = Random.Range(directionChangeInterval * 0.5f, directionChangeInterval * 1.5f);
        }

        // Плавное изменение оси вращения
        currentRotationAxis = Vector3.Slerp(
            currentRotationAxis,
            targetRotationAxis,
            Time.deltaTime / smoothTime
        );

        // Плавное изменение скорости вращения
        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed,
            targetRotationSpeed,
            Time.deltaTime / smoothTime
        );

        // Применение вращения
        float rotationAmount = currentRotationSpeed * Time.deltaTime;
        transform.Rotate(currentRotationAxis, rotationAmount, Space.World);

        // Добавление вобблинга
        if (enableWobble)
        {
            ApplyWobble();
        }
    }

    void GenerateNewTarget()
    {
        // Случайная ось вращения (нормализованная)
        targetRotationAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        // Случайная скорость
        targetRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
    }

    void ApplyWobble()
    {
        // Неравномерное вращение для эффекта дрожания
        float wobbleX = Mathf.PerlinNoise(Time.time * wobbleFrequency, perlinSeed) - 0.5f;
        float wobbleY = Mathf.PerlinNoise(Time.time * wobbleFrequency + 1f, perlinSeed + 10f) - 0.5f;
        float wobbleZ = Mathf.PerlinNoise(Time.time * wobbleFrequency + 2f, perlinSeed + 20f) - 0.5f;

        Vector3 wobbleRotation = new Vector3(wobbleX, wobbleY, wobbleZ) * wobbleIntensity;
        transform.Rotate(wobbleRotation * Time.deltaTime, Space.World);
    }

    // Публичный метод для сброса вращения
    public void ResetRotation()
    {
        transform.rotation = Random.rotation;
        GenerateNewTarget();
        currentRotationAxis = targetRotationAxis;
        currentRotationSpeed = targetRotationSpeed;
    }

    // Публичный метод для установки параметров вращения
    public void SetRotationParameters(float minSpeed, float maxSpeed, float changeInterval)
    {
        minRotationSpeed = minSpeed;
        maxRotationSpeed = maxSpeed;
        directionChangeInterval = changeInterval;
    }
}