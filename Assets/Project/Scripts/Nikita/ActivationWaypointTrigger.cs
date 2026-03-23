using UnityEngine;

public class ActivationWaypointTrigger : MonoBehaviour
{
    [Header("Настройки триггера")]
    [SerializeField] private string targetTag = "Player"; // Тэг объекта, который активирует триггер
    [SerializeField] private bool activateOnEnter = true; // Активировать при входе
    [SerializeField] private bool deactivateOnExit = false; // Деактивировать при выходе
    [SerializeField] private bool oneTimeUse = false; // Триггер срабатывает только один раз
    [SerializeField] private bool requireKeyPress = false; // Требовать нажатие клавиши
    [SerializeField] private KeyCode activationKey = KeyCode.E; // Клавиша активации

    [Header("Целевой WaypointFollower")]
    [SerializeField] private WaypointFollower targetWaypointFollower;

    [Header("Визуальная обратная связь")]
    [SerializeField] private bool showActivationPrompt = true;
    [SerializeField] private string activationMessage = "Нажмите E для активации";
    [SerializeField] private GameObject activationHint; // UI элемент или 3D объект для подсказки

    private bool isPlayerInTrigger = false;
    private bool hasBeenUsed = false;

    void Start()
    {
        // Проверка наличия компонента WaypointFollower
        if (targetWaypointFollower == null)
        {
            // Попытка найти на том же объекте
            targetWaypointFollower = GetComponent<WaypointFollower>();

            // Если не нашли, ищем на родительском объекте
            if (targetWaypointFollower == null && transform.parent != null)
            {
                targetWaypointFollower = transform.parent.GetComponent<WaypointFollower>();
            }

            // Если все еще не нашли, ищем по тегу
            if (targetWaypointFollower == null)
            {
                GameObject waypointObject = GameObject.FindGameObjectWithTag("WaypointFollower");
                if (waypointObject != null)
                {
                    targetWaypointFollower = waypointObject.GetComponent<WaypointFollower>();
                }
            }

            if (targetWaypointFollower == null)
            {
                Debug.LogWarning($"WaypointFollower не найден для {gameObject.name}!");
            }
        }

        // Настройка визуальной обратной связи
        if (activationHint != null)
        {
            activationHint.SetActive(false);
        }
    }

    void Update()
    {
        // Обработка активации по клавише
        if (requireKeyPress && isPlayerInTrigger && !hasBeenUsed)
        {
            if (Input.GetKeyDown(activationKey))
            {
                ActivateWaypointFollower();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем тег объекта, вошедшего в триггер
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = true;

            // Показываем подсказку если нужно
            if (showActivationPrompt && activationHint != null)
            {
                activationHint.SetActive(true);
            }

            // Если не требуется нажатие клавиши - активируем сразу
            if (activateOnEnter && !requireKeyPress && !hasBeenUsed)
            {
                ActivateWaypointFollower();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Проверяем тег объекта, вышедшего из триггера
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = false;

            // Скрываем подсказку
            if (activationHint != null)
            {
                activationHint.SetActive(false);
            }

            // Деактивируем если нужно
            if (deactivateOnExit && targetWaypointFollower != null)
            {
                targetWaypointFollower.StopMoving();
            }
        }
    }

    void ActivateWaypointFollower()
    {
        if (targetWaypointFollower == null)
        {
            Debug.LogError($"Не могу активировать WaypointFollower - объект не назначен!");
            return;
        }

        if (oneTimeUse && hasBeenUsed)
        {
            Debug.Log($"Триггер {gameObject.name} уже использован!");
            return;
        }

        // Активируем WaypointFollower
        targetWaypointFollower.StartMoving();
        Debug.Log($"WaypointFollower активирован триггером {gameObject.name}");

        // Помечаем как использованный если одноразовый
        if (oneTimeUse)
        {
            hasBeenUsed = true;

            // Отключаем визуальную подсказку
            if (activationHint != null)
            {
                activationHint.SetActive(false);
            }

            // Отключаем сам триггер если нужно
            if (GetComponent<Collider>() != null)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация в редакторе
        if (targetWaypointFollower != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetWaypointFollower.transform.position);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    // === Публичные методы для ручного управления ===

    /// <summary>
    /// Ручная активация WaypointFollower (можно вызывать из других скриптов)
    /// </summary>
    public void ManualActivate()
    {
        ActivateWaypointFollower();
    }

    /// <summary>
    /// Ручная деактивация WaypointFollower
    /// </summary>
    public void ManualDeactivate()
    {
        if (targetWaypointFollower != null)
        {
            targetWaypointFollower.StopMoving();
        }
    }

    /// <summary>
    /// Сброс состояния триггера (полезно для многоразовых триггеров)
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isPlayerInTrigger = false;

        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = true;
        }
    }

    /// <summary>
    /// Установить новый WaypointFollower
    /// </summary>
    public void SetTargetWaypointFollower(WaypointFollower newTarget)
    {
        targetWaypointFollower = newTarget;
    }

    /// <summary>
    /// Получить текущий WaypointFollower
    /// </summary>
    public WaypointFollower GetTargetWaypointFollower()
    {
        return targetWaypointFollower;
    }
}