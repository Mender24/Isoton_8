using UnityEngine;
using System.Collections;

public class RigidbodyPusherWithDelay : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
    [SerializeField] private bool useLocalDirection = true;
    [SerializeField] private Vector3 customDirection = Vector3.forward;

    [Header("Delay Settings")]
    [SerializeField] private float delayBeforePush = 1f; // Задержка в секундах
    [SerializeField] private bool useRandomDelay = false;
    [SerializeField] private Vector2 randomDelayRange = new Vector2(0.5f, 2f);

    [Header("Optional Settings")]
    [SerializeField] private bool resetVelocityBeforePush = false;
    [SerializeField] private bool addTorque = false;
    [SerializeField] private float torqueForce = 5f;

    [Header("Visual/Audio Feedback")]
    [SerializeField] private ParticleSystem pushEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pushSound;

    private Rigidbody rb;
    private bool hasPushed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"Rigidbody not found on {gameObject.name}!");
        }
    }

    private void Start()
    {
        StartCoroutine(PushAfterDelay());
    }

    private IEnumerator PushAfterDelay()
    {
        // Определяем время задержки
        float actualDelay = useRandomDelay
            ? Random.Range(randomDelayRange.x, randomDelayRange.y)
            : delayBeforePush;

        // Ждем указанное время
        yield return new WaitForSeconds(actualDelay);

        // Выполняем толчок
        Push();
    }

    // Метод для вызова из других скриптов или событий
    public void Push()
    {
        if (rb == null || hasPushed) return;

        hasPushed = true;

        // Опционально сбрасываем скорость
        if (resetVelocityBeforePush)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Определяем направление толчка
        Vector3 pushDirection;

        if (useLocalDirection)
        {
            pushDirection = transform.forward;
        }
        else
        {
            pushDirection = customDirection.normalized;
        }

        // Применяем силу
        rb.AddForce(pushDirection * pushForce, forceMode);

        // Опционально добавляем вращение
        if (addTorque)
        {
            rb.AddTorque(transform.right * torqueForce, ForceMode.Impulse);
        }

        // Визуальные/звуковые эффекты
        PlayFeedback();
    }

    private void PlayFeedback()
    {
        if (pushEffect != null)
        {
            pushEffect.Play();
        }

        if (audioSource != null && pushSound != null)
        {
            audioSource.PlayOneShot(pushSound);
        }
    }

    // Метод для толчка с кастомной силой
    public void PushWithCustomForce(float customForce)
    {
        if (rb == null) return;
        pushForce = customForce;
        Push();
    }

    // Метод для изменения задержки
    public void SetDelay(float newDelay)
    {
        delayBeforePush = newDelay;
    }

    // Сброс состояния для повторного использования
    public void ResetPushState()
    {
        hasPushed = false;
    }
}