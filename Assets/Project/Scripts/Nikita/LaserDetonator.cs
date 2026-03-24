using UnityEngine;

namespace Akila.FPSFramework
{
    public class LaserDetonator : MonoBehaviour
    {
        [Header("Laser Settings")]
        public float laserMaxDistance = 20f;
        public LayerMask targetLayerMask = -1; // Что может активировать лазер
        public float checkInterval = 0.1f; // Частота проверки

        [Header("Target Layers")]
        public bool triggerOnDefaultLayer = true;
        public bool triggerOnPlayerLayer = true;
        public bool triggerOnEnemyLayer = true;
        public LayerMask customTargetLayers = 0; // Дополнительные слои для триггера

        [Header("Laser Visuals")]
        public LineRenderer laserLine;
        public Material laserMaterial;
        public Color activeColor = Color.red;
        public Color triggeredColor = Color.green;

        [Header("Explosive Reference")]
        public Explosive explosive; // Ссылка на взрывчатку
        public bool autoFindExplosive = true;

        [Header("Triggers")]
        public bool destroyLaserOnTrigger = true;
        public bool triggerOnlyOnce = true;

        [Header("Debug")]
        public bool showRayInDebug = true;

        private bool isTriggered = false;
        private bool isActive = true;
        private RaycastHit lastHit;

        // Кэшируем битовые маски слоев для быстрой проверки
        private int defaultLayerMask;
        private int playerLayerMask;
        private int enemyLayerMask;

        private void Start()
        {
            if (autoFindExplosive && explosive == null)
                explosive = GetComponent<Explosive>();

            if (explosive == null)
                Debug.LogError("LaserDetonator: No Explosive component found!");

            // Получаем битовые маски слоев
            defaultLayerMask = 1 << LayerMask.NameToLayer("Default");
            playerLayerMask = 1 << LayerMask.NameToLayer("Player");
            enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");

            // Если слои не существуют, логируем предупреждение
            if (LayerMask.NameToLayer("Default") == -1)
                Debug.LogWarning("LaserDetonator: Layer 'Default' not found!");
            if (LayerMask.NameToLayer("Player") == -1)
                Debug.LogWarning("LaserDetonator: Layer 'Player' not found!");
            if (LayerMask.NameToLayer("Enemy") == -1)
                Debug.LogWarning("LaserDetonator: Layer 'Enemy' not found!");

            SetupLaserVisual();

            // Запускаем периодическую проверку
            InvokeRepeating(nameof(CheckLaserTrigger), 0f, checkInterval);
        }

        private void SetupLaserVisual()
        {
            if (laserLine == null)
                laserLine = GetComponent<LineRenderer>();

            if (laserLine == null)
                laserLine = gameObject.AddComponent<LineRenderer>();

            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.material = laserMaterial;
            laserLine.startColor = activeColor;
            laserLine.endColor = activeColor;
        }

        private void Update()
        {
            UpdateLaserVisual();
        }

        private void CheckLaserTrigger()
        {
            if (!isActive || isTriggered || explosive == null || explosive.exploded)
                return;

            Vector3 laserDirection = transform.forward;

            // Raycast для проверки пересечения лазера с целью
            RaycastHit[] hits = Physics.RaycastAll(transform.position, laserDirection, laserMaxDistance, targetLayerMask);

            foreach (RaycastHit hit in hits)
            {
                if (ShouldTriggerOnTarget(hit.collider.gameObject.layer))
                {
                    lastHit = hit;
                    TriggerExplosive();
                    break;
                }
            }
        }

        private bool ShouldTriggerOnTarget(int layer)
        {
            int layerBit = 1 << layer;

            // Проверка на слой Default
            if (triggerOnDefaultLayer && (layerBit & defaultLayerMask) != 0)
                return true;

            // Проверка на слой Player
            if (triggerOnPlayerLayer && (layerBit & playerLayerMask) != 0)
                return true;

            // Проверка на слой Enemy
            if (triggerOnEnemyLayer && (layerBit & enemyLayerMask) != 0)
                return true;

            // Проверка на кастомные слои
            if ((layerBit & customTargetLayers) != 0)
                return true;

            return false;
        }

        private void TriggerExplosive()
        {
            if (triggerOnlyOnce && isTriggered)
                return;

            isTriggered = true;

            // Меняем цвет лазера
            if (laserLine != null)
            {
                laserLine.startColor = triggeredColor;
                laserLine.endColor = triggeredColor;
            }

            // Взрываем
            if (explosive != null)
            {
                explosive.Explode();
            }

            // Уничтожаем лазер, если нужно
            if (destroyLaserOnTrigger)
            {
                Destroy(gameObject, 0.1f);
            }
        }

        private void UpdateLaserVisual()
        {
            if (laserLine == null) return;

            Vector3 endPoint = transform.position + transform.forward * laserMaxDistance;

            // Обновляем визуализацию лазера
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, laserMaxDistance))
            {
                endPoint = hit.point;

                // Показываем, что лазер перекрыт целью
                if (ShouldTriggerOnTarget(hit.collider.gameObject.layer))
                {
                    laserLine.startColor = triggeredColor;
                    laserLine.endColor = triggeredColor;
                }
                else
                {
                    laserLine.startColor = activeColor;
                    laserLine.endColor = activeColor;
                }
            }
            else
            {
                laserLine.startColor = activeColor;
                laserLine.endColor = activeColor;
            }

            laserLine.SetPosition(0, transform.position);
            laserLine.SetPosition(1, endPoint);
        }

        // Визуализация в редакторе
        private void OnDrawGizmosSelected()
        {
            if (!showRayInDebug) return;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * laserMaxDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }

        // Метод для активации/деактивации лазера
        public void SetActive(bool active)
        {
            isActive = active;
            if (laserLine != null)
                laserLine.enabled = active;
        }

        // Метод для сброса триггера
        public void ResetTrigger()
        {
            isTriggered = false;
        }

        // Метод для добавления кастомного слоя в триггер
        public void AddCustomLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                customTargetLayers |= (1 << layer);
            }
            else
            {
                Debug.LogWarning($"Layer '{layerName}' not found!");
            }
        }

        // Метод для удаления кастомного слоя из триггера
        public void RemoveCustomLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
            {
                customTargetLayers &= ~(1 << layer);
            }
        }
    }
}