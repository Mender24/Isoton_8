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
    [Space]
    [SerializeField] private float _speedChangeValueUp;
    [SerializeField] private float _speedChangeValueDown;


    private float currentAmplitude = 0;
    private bool isStop = false;

    private float randomOffset;
    private float randomPhase;
    private float originalY;
    private Transform drillTransform;
    private DrillController drillController;

    private void OnDestroy()
    {
        drillController.Started -= OnStarted;
        drillController.Stopped -= OnStopped;
    }

    void Start()
    {
        drillTransform = transform;
        originalY = drillTransform.localPosition.y;
        randomPhase = Random.Range(0f, Mathf.PI * 2);
        randomOffset = Random.Range(-currentAmplitude * randomIntensity, currentAmplitude * randomIntensity);
        drillController = transform.parent.parent.GetComponent<DrillController>();
        drillController.Started += OnStarted;
        drillController.Stopped += OnStopped;
    }

    void Update()
    {
        ChangeAmplitude();

        // Случайная смена крайних точек
        if (Random.value < 0.02f) // 2% шанс каждый кадр
        {
            randomOffset = Random.Range(-currentAmplitude * randomIntensity, currentAmplitude * randomIntensity);
        }

        // Расчёт позиции
        float yOffset = Mathf.Sin(Time.time * speed + randomPhase) * currentAmplitude + randomOffset;

        Vector3 newPos = drillTransform.localPosition;
        newPos.y = originalY + yOffset;
        drillTransform.localPosition = newPos;
    }

    private void ChangeAmplitude()
    {
        float targetAmplitude;

        if (!isStop)
            targetAmplitude = maxAmplitude;
        else
            targetAmplitude = 0;

        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, (isStop ? _speedChangeValueDown : _speedChangeValueUp) * Time.deltaTime);
    }

    private void OnStarted()
    {
        isStop = false;
    }

    private void OnStopped()
    {
        isStop = true;
    }
}
