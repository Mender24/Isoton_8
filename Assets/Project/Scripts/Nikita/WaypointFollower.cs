using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [Header("Настройки пути")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool loop = true;

    [Header("Активация")]
    [SerializeField] private bool isActive = false;

    private int currentWaypointIndex = 0;

    void Start()
    {
        // Проверка на минимальное количество точек
        if (waypoints.Count < 2)
        {
            Debug.LogWarning("Добавьте минимум 2 waypoint для работы!");
            enabled = false;
            return;
        }

        // Если активен при старте, начинаем движение
        if (isActive)
        {
            StartMoving();
        }
    }

    void Update()
    {
        if (!isActive || waypoints.Count < 2)
            return;

        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        // Проверка валидности текущего чекпоинта
        if (currentWaypointIndex >= waypoints.Count || waypoints[currentWaypointIndex] == null)
            return;

        // Получаем позицию целевого чекпоинта
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;

        // Движение к точке с постоянной скоростью
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // Проверка достижения чекпоинта
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            ReachedWaypoint();
        }
    }

    private void ReachedWaypoint()
    {
        // Переходим к следующей точке
        currentWaypointIndex++;

        // Проверка конца маршрута
        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loop)
            {
                // Начинаем сначала
                currentWaypointIndex = 0;
            }
            else
            {
                // Останавливаемся на последней точке
                isActive = false;
            }
        }
    }

    // === Публичные методы ===

    /// <summary>
    /// Начать движение
    /// </summary>
    public void StartMoving()
    {
        isActive = true;
        Debug.Log("Движение начато");
    }

    /// <summary>
    /// Остановить движение
    /// </summary>
    public void StopMoving()
    {
        isActive = false;
        Debug.Log("Движение остановлено");
    }

    /// <summary>
    /// Включить/выключить движение
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        Debug.Log($"Движение {(isActive ? "включено" : "выключено")}");
    }

    /// <summary>
    /// Переключить движение
    /// </summary>
    public void ToggleActive()
    {
        isActive = !isActive;
        Debug.Log($"Движение {(isActive ? "включено" : "выключено")}");
    }

    /// <summary>
    /// Установить новую скорость
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0, newSpeed);
        Debug.Log($"Скорость установлена: {speed}");
    }

    /// <summary>
    /// Перейти к определенному чекпоинту
    /// </summary>
    public void JumpToWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            currentWaypointIndex = index;
            transform.position = waypoints[currentWaypointIndex].position;
            Debug.Log($"Переход к чекпоинту {index}");
        }
    }

    /// <summary>
    /// Начать движение с начала пути
    /// </summary>
    public void StartFromBeginning()
    {
        currentWaypointIndex = 0;
        isActive = true;

        if (waypoints.Count > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
        Debug.Log("Движение начато с первого чекпоинта");
    }

    /// <summary>
    /// Добавить чекпоинт в список
    /// </summary>
    public void AddWaypoint(Transform newWaypoint)
    {
        if (newWaypoint != null)
        {
            waypoints.Add(newWaypoint);
            Debug.Log($"Добавлен новый чекпоинт. Всего: {waypoints.Count}");
        }
    }

    /// <summary>
    /// Удалить чекпоинт по индексу
    /// </summary>
    public void RemoveWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
        {
            waypoints.RemoveAt(index);
            Debug.Log($"Чекпоинт {index} удален. Осталось: {waypoints.Count}");
        }
    }

    /// <summary>
    /// Получить текущий индекс чекпоинта
    /// </summary>
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }

    /// <summary>
    /// Получить текущее состояние активности
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Получить общее количество чекпоинтов
    /// </summary>
    public int GetWaypointCount()
    {
        return waypoints.Count;
    }
}