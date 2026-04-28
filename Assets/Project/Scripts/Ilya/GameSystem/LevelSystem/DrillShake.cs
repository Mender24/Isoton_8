using UnityEngine;

public class DrillShake : MonoBehaviour
{
    [Header("Параметры тряски")]
    [Tooltip("Максимальная амплитуда колебаний (вверх/вниз)")]
    public float maxAmplitude = 0.1f;

    [Tooltip("Скорость движения (частота колебаний)")]
    public float speed = 10f;

    [Tooltip("Интенсивность рандома крайних точек (0-1)")]
    [Range(0, 1)]
    public float randomIntensity = 0.8f;

    private float randomOffset;
    private float randomPhase;
    private float originalY;
    private Transform drillTransform;

    void Start()
    {
        drillTransform = transform;
        originalY = drillTransform.localPosition.y;
        randomPhase = Random.Range(0f, Mathf.PI * 2);
        randomOffset = Random.Range(-maxAmplitude * randomIntensity, maxAmplitude * randomIntensity);
    }

    void Update()
    {
        // Случайная смена крайних точек
        if (Random.value < 0.02f) // 2% шанс каждый кадр
        {
            randomOffset = Random.Range(-maxAmplitude * randomIntensity, maxAmplitude * randomIntensity);
        }

        // Расчёт позиции
        float yOffset = Mathf.Sin(Time.time * speed + randomPhase) * maxAmplitude + randomOffset;

        Vector3 newPos = drillTransform.localPosition;
        newPos.y = originalY + yOffset;
        drillTransform.localPosition = newPos;
    }
}
