using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PairLightActivator : MonoBehaviour
{
    [Header("Настройки источников света")]
    [Tooltip("Массив объектов с компонентом Light")]
    public Light[] lights;

    [Header("Настройки активации")]
    [Tooltip("Минимальная задержка между включением пар (сек)")]
    public float minDelay = 0.2f;

    [Tooltip("Максимальная задержка между включением пар (сек)")]
    public float maxDelay = 0.2f; // По умолчанию 0.2, можно изменить для вариативности

    private bool isActivating = false;

    private void Start()
    {
        // Проверяем, что все объекты действительно имеют компонент Light
        ValidateLights();
    }

    // Метод для валидации источников света
    private void ValidateLights()
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogWarning("Массив lights пуст! Добавьте объекты с компонентом Light в инспекторе.");
            return;
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null)
            {
                Debug.LogWarning($"Объект на индексе {i} не имеет компонента Light!");
            }
        }
    }

    // Публичный метод для запуска активации (можно вызвать из другого скрипта или через событие)
    public void ActivateLights()
    {
        if (!isActivating && lights != null && lights.Length > 0)
        {
            StartCoroutine(ActivateLightsCoroutine());
        }
    }

    // Публичный метод для остановки активации (если нужно прервать процесс)
    public void StopActivation()
    {
        if (isActivating)
        {
            StopAllCoroutines();
            isActivating = false;
        }
    }

    // Корутина для последовательной активации
    private IEnumerator ActivateLightsCoroutine()
    {
        isActivating = true;

        // Обрабатываем источники света попарно
        for (int i = 0; i < lights.Length; i += 2)
        {
            // Включаем первую лампу в паре
            if (lights[i] != null)
            {
                lights[i].enabled = true;
                Debug.Log($"Включен свет {lights[i].name}");
            }

            // Включаем вторую лампу в паре (если существует)
            if (i + 1 < lights.Length && lights[i + 1] != null)
            {
                lights[i + 1].enabled = true;
                Debug.Log($"Включен свет {lights[i + 1].name}");
            }

            // Если это не последняя пара, ждем перед следующей
            if (i + 2 < lights.Length)
            {
                // Генерируем случайную задержку между minDelay и maxDelay
                float delay = Random.Range(minDelay, maxDelay);
                yield return new WaitForSeconds(delay);
            }
        }

        isActivating = false;
        Debug.Log("Активация всех источников света завершена!");
    }

    // Опционально: метод для сброса (выключения всех источников)
    public void ResetLights()
    {
        StopActivation();

        if (lights != null)
        {
            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }
    }

    // Для удобства тестирования в редакторе
    [ContextMenu("Активировать свет")]
    private void TestActivate()
    {
        ActivateLights();
    }

    [ContextMenu("Сбросить свет")]
    private void TestReset()
    {
        ResetLights();
    }
}