using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Components")]
    [SerializeField] private Light targetLight; // Сама лампочка
    [SerializeField] private bool findLightOnStart = true; // Искать свет автоматически

    [Header("Flicker Settings")]
    [SerializeField] private float minFlickerInterval = 0.05f; // Минимальный интервал мерцания
    [SerializeField] private float maxFlickerInterval = 0.3f; // Максимальный интервал мерцания
    [SerializeField] private float minIntensity = 0f; // Минимальная яркость при мерцании
    [SerializeField] private float maxIntensity = 1f; // Максимальная яркость при мерцании
    [SerializeField] private bool randomIntensity = true; // Случайная яркость или вкл/выкл

    [Header("Stable On Settings")]
    [SerializeField] private float stableOnDelay = 5f; // Задержка перед стабильным включением
    [SerializeField] private float stableIntensity = 1f; // Яркость в стабильном режиме

    // Состояния лампочки
    private bool isFlickering = false;
    private bool isStableOn = false;
    private float originalIntensity;

    void Start()
    {
        // Автоматически ищем компонент Light, если не назначен
        if (targetLight == null && findLightOnStart)
        {
            targetLight = GetComponent<Light>();

            // Если на текущем объекте нет, ищем в дочерних
            if (targetLight == null)
                targetLight = GetComponentInChildren<Light>();
        }

        if (targetLight != null)
        {
            originalIntensity = targetLight.intensity;

            // Изначально свет выключен
            targetLight.enabled = false;
            targetLight.intensity = 0f;

            Debug.Log("Light is OFF. Call TurnOnWithFlicker() to start.");
        }
        else
        {
            Debug.LogError("Light component not found! Please assign it manually.");
        }
    }

    /// <summary>
    /// Публичный метод для включения света с мерцанием
    /// Вызовите этот метод из другого скрипта, чтобы начать процесс
    /// </summary>
    /// <param name="customDelay">Можно указать свою задержку (опционально)</param>
    public void TurnOnWithFlicker(float customDelay = -1f)
    {
        if (isStableOn)
        {
            Debug.Log("Light is already stable ON");
            return;
        }

        if (isFlickering)
        {
            Debug.Log("Light is already flickering");
            return;
        }

        // Используем кастомную задержку, если указана
        float delay = customDelay >= 0 ? customDelay : stableOnDelay;

        StartCoroutine(FlickerThenStable(delay));
    }

    /// <summary>
    /// Альтернативный метод с отдельными параметрами
    /// </summary>
    /// <param name="flickerDuration">Длительность мерцания</param>
    /// <param name="targetIntensity">Итоговая яркость</param>
    public void TurnOnWithFlicker(float flickerDuration, float targetIntensity)
    {
        if (isStableOn)
        {
            Debug.Log("Light is already stable ON");
            return;
        }

        if (isFlickering)
        {
            Debug.Log("Light is already flickering");
            return;
        }

        StartCoroutine(FlickerThenStable(flickerDuration, targetIntensity));
    }

    /// <summary>
    /// Основная корутина: мерцание -> стабильное включение
    /// </summary>
    private IEnumerator FlickerThenStable(float flickerDuration, float? targetIntensity = null)
    {
        isFlickering = true;

        // Включаем свет (он может быть выключен)
        targetLight.enabled = true;

        Debug.Log($"Light started flickering for {flickerDuration} seconds");

        float startTime = Time.time;

        // Мерцаем пока не пройдет flickerDuration
        while (Time.time - startTime < flickerDuration)
        {
            if (randomIntensity)
            {
                // Случайная яркость от 0 до максимальной
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);
            }
            else
            {
                // Просто вкл/выкл
                targetLight.enabled = !targetLight.enabled;
            }

            // Ждем случайный интервал
            float flickerTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            yield return new WaitForSeconds(flickerTime);
        }

        // Завершаем мерцание и включаем стабильно
        isFlickering = false;
        isStableOn = true;

        // Устанавливаем итоговую яркость
        targetLight.intensity = targetIntensity ?? stableIntensity;
        targetLight.enabled = true;

        Debug.Log($"Light is now stable ON with intensity {targetLight.intensity}");
    }

    /// <summary>
    /// Немедленно выключить свет
    /// </summary>
    public void TurnOff()
    {
        StopAllCoroutines();
        isFlickering = false;
        isStableOn = false;

        if (targetLight != null)
        {
            targetLight.enabled = false;
            targetLight.intensity = 0f;
        }

        Debug.Log("Light turned OFF");
    }

    /// <summary>
    /// Проверить состояние лампочки
    /// </summary>
    public bool IsStableOn()
    {
        return isStableOn;
    }

    /// <summary>
    /// Проверить, мерцает ли лампочка
    /// </summary>
    public bool IsFlickering()
    {
        return isFlickering;
    }

    /// <summary>
    /// Получить текущую яркость
    /// </summary>
    public float GetCurrentIntensity()
    {
        return targetLight != null ? targetLight.intensity : 0f;
    }

    /// <summary>
    /// Визуализация в редакторе
    /// </summary>
    private void OnValidate()
    {
        if (minFlickerInterval > maxFlickerInterval)
            minFlickerInterval = maxFlickerInterval;

        if (minIntensity > maxIntensity)
            minIntensity = maxIntensity;
    }
}