using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Components")]
    [SerializeField] private Light targetLight;
    [SerializeField] private bool findLightOnStart = true;

    [Header("Flicker Settings")]
    [SerializeField] private float minFlickerInterval = 0.02f; // Минимальный интервал (уменьшен)
    [SerializeField] private float maxFlickerInterval = 0.15f; // Максимальный интервал (уменьшен)
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 0.8f; // Уменьшена максимальная яркость
    [SerializeField] private bool randomIntensity = true;
    
    [Header("Flicker Behavior")]
    [SerializeField] [Range(0f, 1f)] private float offChance = 0.4f; // Шанс полного выключения (40%)
    [SerializeField] private float offDuration = 0.1f; // Длительность полного выключения

    [Header("Stable On Settings")]
    [SerializeField] private float stableOnDelay = 5f;
    [SerializeField] private float stableIntensity = 1f;

    private bool isFlickering = false;
    private bool isStableOn = false;
    private float originalIntensity;

    void Start()
    {
        if (targetLight == null && findLightOnStart)
        {
            targetLight = GetComponent<Light>();
            if (targetLight == null)
                targetLight = GetComponentInChildren<Light>();
        }

        if (targetLight != null)
        {
            originalIntensity = targetLight.intensity;
            targetLight.enabled = false;
            targetLight.intensity = 0f;
            Debug.Log("Light is OFF. Waiting for trigger...");
        }
        else
        {
            Debug.LogError("Light component not found! Please assign it manually.");
        }
    }

    public void ActivateLight()
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

        StartCoroutine(FlickerThenStable());
    }

    private IEnumerator FlickerThenStable()
    {
        isFlickering = true;
        targetLight.enabled = true;

        Debug.Log($"Light started flickering for {stableOnDelay} seconds");

        float startTime = Time.time;

        while (Time.time - startTime < stableOnDelay)
        {
            // Проверяем, нужно ли полностью выключить свет
            if (Random.value < offChance)
            {
                // Полное выключение
                targetLight.intensity = 0f;
                targetLight.enabled = false;
                yield return new WaitForSeconds(offDuration);
                
                // Включаем обратно с низкой яркостью
                targetLight.enabled = true;
                targetLight.intensity = Random.Range(0.1f, 0.4f);
            }
            else if (randomIntensity)
            {
                // Случайная яркость, но с большим шансом низких значений
                float intensityBias = Random.value; // 0-1
                if (intensityBias < 0.6f) // 60% шанс низкой яркости
                {
                    targetLight.intensity = Random.Range(minIntensity, 0.3f);
                }
                else // 40% шанс средней яркости
                {
                    targetLight.intensity = Random.Range(0.3f, maxIntensity);
                }
            }
            else
            {
                // Просто вкл/выкл (если randomIntensity = false)
                targetLight.enabled = !targetLight.enabled;
            }

            // Случайная задержка между изменениями
            float flickerTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            yield return new WaitForSeconds(flickerTime);
        }

        // Стабильное включение
        isFlickering = false;
        isStableOn = true;
        targetLight.intensity = stableIntensity;
        targetLight.enabled = true;

        Debug.Log($"Light is now stable ON with intensity {stableIntensity}");
    }

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

    private void OnValidate()
    {
        if (minFlickerInterval > maxFlickerInterval)
            minFlickerInterval = maxFlickerInterval;

        if (minIntensity > maxIntensity)
            minIntensity = maxIntensity;
    }
}