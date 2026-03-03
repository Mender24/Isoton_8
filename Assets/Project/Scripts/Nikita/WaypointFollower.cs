using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [Header("Настройки движения к конкретному waypoint")]
    [SerializeField] private Transform targetWaypoint;
    [SerializeField] private float maxSpeed = 5f;

    [Header("Настройки ускорения и замедления")]
    [SerializeField] private float accelerationTime = 2f; // Время разгона
    [SerializeField] private float decelerationTime = 1.5f; // Время замедления
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Кривая замедления

    [Header("Настройки торможения")]
    [SerializeField] private float startDecelerationDistance = 3f; // Расстояние, на котором начинается замедление
    [SerializeField] private float stoppingDistance = 0.1f; // Расстояние полной остановки
    [SerializeField] private bool smoothStop = true; // Плавная остановка

    [Header("Активация")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Текущее состояние")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float accelerationProgress = 0f;
    [SerializeField] private float decelerationProgress = 0f;
    [SerializeField] private bool hasReachedTarget = false;
    [SerializeField] private bool isDecelerating = false;

    private float startDelayTimer = 0f;
    private bool isWaitingForStart = false;
    private Vector3 lastTargetPosition;
    private float distanceToTarget;

    void Update()
    {
        if (!isActive || targetWaypoint == null)
            return;

        // Обработка задержки перед стартом
        if (isWaitingForStart)
        {
            HandleStartDelay();
            return;
        }

        // Если уже достигли цели
        if (hasReachedTarget)
            return;

        // Обновляем расстояние до цели
        distanceToTarget = Vector3.Distance(transform.position, targetWaypoint.position);

        // Проверка достижения цели
        if (distanceToTarget <= stoppingDistance)
        {
            ReachedTarget();
            return;
        }

        // Определяем, нужно ли начинать замедление
        bool shouldDecelerate = distanceToTarget <= startDecelerationDistance;

        // Обновление ускорения/замедления
        if (shouldDecelerate)
        {
            UpdateDeceleration();
        }
        else
        {
            UpdateAcceleration();
        }

        // Движение к целевому waypoint
        MoveToWaypoint();
    }

    private void HandleStartDelay()
    {
        startDelayTimer -= Time.deltaTime;

        if (startDelayTimer <= 0)
        {
            isWaitingForStart = false;
            accelerationProgress = 0f;
            decelerationProgress = 0f;
            currentSpeed = 0f;
            isDecelerating = false;
            hasReachedTarget = false;
            Debug.Log("Задержка завершена. Начинаем движение!");
        }
    }

    private void UpdateAcceleration()
    {
        isDecelerating = false;

        // Увеличиваем прогресс ускорения
        if (accelerationTime > 0 && accelerationProgress < 1f)
        {
            accelerationProgress += Time.deltaTime / accelerationTime;
            accelerationProgress = Mathf.Clamp01(accelerationProgress);
        }

        // Сбрасываем прогресс замедления
        decelerationProgress = 0f;

        // Вычисляем текущую скорость по кривой ускорения
        float accelerationFactor = accelerationCurve.Evaluate(accelerationProgress);
        currentSpeed = maxSpeed * accelerationFactor;
    }

    private void UpdateDeceleration()
    {
        if (!smoothStop)
        {
            // Простое линейное замедление
            float t = 1f - Mathf.Clamp01(distanceToTarget / startDecelerationDistance);
            currentSpeed = Mathf.Lerp(maxSpeed, 0f, t);
            return;
        }

        isDecelerating = true;

        // Прогресс замедления на основе расстояния
        float targetDecelerationProgress = 1f - Mathf.Clamp01(distanceToTarget / startDecelerationDistance);

        // Плавно увеличиваем прогресс замедления
        if (decelerationTime > 0)
        {
            float decelerationSpeed = 1f / decelerationTime;
            decelerationProgress = Mathf.MoveTowards(decelerationProgress, targetDecelerationProgress,
                                                     decelerationSpeed * Time.deltaTime);
        }
        else
        {
            decelerationProgress = targetDecelerationProgress;
        }

        // Сбрасываем прогресс ускорения
        accelerationProgress = 0f;

        // Вычисляем текущую скорость по кривой замедления
        float decelerationFactor = decelerationCurve.Evaluate(decelerationProgress);
        currentSpeed = maxSpeed * decelerationFactor;
    }

    private void MoveToWaypoint()
    {
        // Проверка на движение цели
        if (targetWaypoint.position != lastTargetPosition)
        {
            // Если цель двигается, корректируем параметры
            if (!isDecelerating)
            {
                accelerationProgress = Mathf.Max(0, accelerationProgress - 0.1f);
            }
        }
        lastTargetPosition = targetWaypoint.position;

        // Получаем направление к цели
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;

        // Рассчитываем шаг движения
        float step = currentSpeed * Time.deltaTime;

        // Проверяем, не превышает ли шаг расстояние до цели
        if (step > distanceToTarget)
        {
            transform.position = targetWaypoint.position;
        }
        else
        {
            // Плавное движение
            transform.position += direction * step;
        }

        // Визуальная отладка (опционально)
        Debug.DrawLine(transform.position, targetWaypoint.position,
                       isDecelerating ? Color.red : Color.green);
    }

    private void ReachedTarget()
    {
        hasReachedTarget = true;
        currentSpeed = 0f;
        accelerationProgress = 0f;
        decelerationProgress = 0f;
        isDecelerating = false;
        transform.position = targetWaypoint.position; // Точное позиционирование

        Debug.Log($"Достигнут целевой waypoint: {targetWaypoint.name}");
        OnTargetReached();
    }

    /// <summary>
    /// Вызывается при достижении цели
    /// </summary>
    private void OnTargetReached()
    {
        // Здесь можно добавить дополнительную логику при достижении цели
        // Например, активацию событий, анимаций и т.д.
    }

    // === Публичные методы ===

    /// <summary>
    /// Установить новый целевой waypoint и начать движение к нему
    /// </summary>
    public void SetTargetWaypoint(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("Целевой waypoint не может быть null!");
            return;
        }

        targetWaypoint = newTarget;
        ResetMovement();
        Debug.Log($"Установлен новый целевой waypoint: {newTarget.name}");
    }

    /// <summary>
    /// Начать движение к текущему целевому waypoint
    /// </summary>
    public void StartMoving()
    {
        if (targetWaypoint == null)
        {
            Debug.LogWarning("Не установлен целевой waypoint!");
            return;
        }

        isActive = true;
        hasReachedTarget = false;

        if (startDelay > 0)
        {
            isWaitingForStart = true;
            startDelayTimer = startDelay;
            Debug.Log($"Движение к {targetWaypoint.name} начнется через {startDelay} секунд...");
        }
        else
        {
            isWaitingForStart = false;
            accelerationProgress = 0f;
            decelerationProgress = 0f;
            currentSpeed = 0f;
            isDecelerating = false;
            Debug.Log($"Движение к {targetWaypoint.name} начато!");
        }
    }

    /// <summary>
    /// Начать движение к конкретному waypoint
    /// </summary>
    public void MoveToTarget(Transform target)
    {
        SetTargetWaypoint(target);
        StartMoving();
    }

    /// <summary>
    /// Начать движение к конкретному waypoint с указанной скоростью
    /// </summary>
    public void MoveToTarget(Transform target, float speed)
    {
        maxSpeed = speed;
        SetTargetWaypoint(target);
        StartMoving();
    }

    /// <summary>
    /// Начать движение к конкретному waypoint с указанными параметрами
    /// </summary>
    public void MoveToTarget(Transform target, float speed, float delay, float accelTime, float decelTime = -1f)
    {
        maxSpeed = speed;
        startDelay = delay;
        accelerationTime = accelTime;

        if (decelTime > 0)
        {
            decelerationTime = decelTime;
        }

        SetTargetWaypoint(target);
        StartMoving();
    }

    /// <summary>
    /// Остановить движение
    /// </summary>
    public void StopMoving()
    {
        isActive = false;
        isWaitingForStart = false;
        currentSpeed = 0f;
        accelerationProgress = 0f;
        decelerationProgress = 0f;
        isDecelerating = false;
        Debug.Log("Движение остановлено");
    }

    /// <summary>
    /// Сбросить состояние движения
    /// </summary>
    public void ResetMovement()
    {
        StopMoving();
        hasReachedTarget = false;
    }

    /// <summary>
    /// Проверить, достигнут ли целевой waypoint
    /// </summary>
    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }

    /// <summary>
    /// Получить расстояние до цели
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (targetWaypoint == null)
            return -1f;

        return Vector3.Distance(transform.position, targetWaypoint.position);
    }

    /// <summary>
    /// Получить текущую скорость
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    /// <summary>
    /// Получить текущий целевой waypoint
    /// </summary>
    public Transform GetTargetWaypoint()
    {
        return targetWaypoint;
    }

    /// <summary>
    /// Установить максимальную скорость
    /// </summary>
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = Mathf.Max(0, speed);
    }

    /// <summary>
    /// Установить время ускорения
    /// </summary>
    public void SetAccelerationTime(float time)
    {
        accelerationTime = Mathf.Max(0, time);
    }

    /// <summary>
    /// Установить время замедления
    /// </summary>
    public void SetDecelerationTime(float time)
    {
        decelerationTime = Mathf.Max(0, time);
    }

    /// <summary>
    /// Установить дистанцию начала торможения
    /// </summary>
    public void SetStartDecelerationDistance(float distance)
    {
        startDecelerationDistance = Mathf.Max(0, distance);
    }

    /// <summary>
    /// Установить задержку старта
    /// </summary>
    public void SetStartDelay(float delay)
    {
        startDelay = Mathf.Max(0, delay);
    }

    /// <summary>
    /// Проверить, замедляется ли объект
    /// </summary>
    public bool IsDecelerating()
    {
        return isDecelerating;
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (targetWaypoint != null)
        {
            // Рисуем линию к целевому waypoint
            Gizmos.color = isDecelerating ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, targetWaypoint.position);

            // Рисуем сферу на целевом waypoint
            Gizmos.color = hasReachedTarget ? Color.green : Color.red;
            Gizmos.DrawSphere(targetWaypoint.position, 0.3f);

            // Рисуем окружность stopping distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetWaypoint.position, stoppingDistance);

            // Рисуем окружность начала торможения
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetWaypoint.position, startDecelerationDistance);

            // Визуализируем текущую скорость (линия от объекта)
            Gizmos.color = Color.white;
            Vector3 speedDirection = (targetWaypoint.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, speedDirection * (currentSpeed / maxSpeed) * 2f);
        }
    }
}