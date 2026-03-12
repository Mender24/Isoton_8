using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [Header("Настройки триггера")]
    [Tooltip("Тег объекта, который считается игроком")]
    public string playerTag = "Player";

    [Tooltip("Можно ли активировать только один раз")]
    public bool oneTimeOnly = true;

    [Header("Событие при входе в триггер")]
    public UnityEvent onPlayerEnter;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, является ли вошедший объект игроком
        if (other.CompareTag(playerTag))
        {
            // Проверяем, можно ли активировать (если oneTimeOnly)
            if (oneTimeOnly && alreadyTriggered)
                return;

            // Вызываем все методы, назначенные в инспекторе
            onPlayerEnter.Invoke();

            alreadyTriggered = true;
        }
    }

    // Опционально: сброс состояния для повторного использования
    public void ResetTrigger()
    {
        alreadyTriggered = false;
    }
}