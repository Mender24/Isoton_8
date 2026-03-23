using UnityEngine;

public class AutoDeactivate : MonoBehaviour
{
    [Tooltip("Время в секундах через которое объект деактивируется")]
    public float deactivateTime = 3f;

    private float timer;

    // Этот метод вызывается автоматически при активации объекта
    private void OnEnable()
    {
        // Сбрасываем таймер при включении объекта
        timer = 0f;
    }

    private void Update()
    {
        // Увеличиваем таймер на время, прошедшее с прошлого кадра
        timer += Time.deltaTime;

        // Если прошло нужное количество времени
        if (timer >= deactivateTime)
        {
            // Деактивируем объект
            gameObject.SetActive(false);
        }
    }
}