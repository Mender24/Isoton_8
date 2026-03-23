using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;

    // Публичный метод для запуска движения
    public void StartMoving()
    {
        isMoving = true;
    }

    // Публичный метод для остановки движения
    public void StopMoving()
    {
        isMoving = false;
    }

    // Публичный метод для изменения скорости
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    void Update()
    {
        if (isMoving)
        {
            // Двигаем объект вперед по его локальной оси Z
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
}