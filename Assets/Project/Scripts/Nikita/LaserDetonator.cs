using UnityEngine;

namespace Akila.FPSFramework
{
    public class LaserDetonator : MonoBehaviour
    {
        [Header("Settings")]
        public float laserDistance = 20f;
        public Explosive explosive;

        [Header("Visuals")]
        public LineRenderer laserLine;
        public Material laserMaterial;

        private bool exploded = false;

        void Start()
        {
            // Находим взрывчатку если не назначена
            if (explosive == null)
                explosive = GetComponent<Explosive>();

            // Создаем лазер если нет
            if (laserLine == null)
                laserLine = gameObject.AddComponent<LineRenderer>();

            // Настройки лазера
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.material = laserMaterial;
        }

        void Update()
        {
            if (exploded) return;
            if (explosive == null) return;
            if (explosive.exploded) return;

            // Пускаем луч
            RaycastHit hit;
            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(origin, direction, out hit, laserDistance))
            {
                // Рисуем лазер до точки попадания
                DrawLaser(origin, hit.point);

                // Проверяем слой
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                // Меняем цвет в зависимости от слоя
                if (layerName == "Environment")
                {
                    SetLaserColor(Color.red);
                }
                else if (layerName == "Default")
                {
                    SetLaserColor(Color.green);
                }
                else if (layerName == "Player")
                {
                    SetLaserColor(Color.red);
                    // Взрываемся
                    explosive.Explode();
                    exploded = true;
                }
                else if (layerName == "Mutant") //реакт на мутанта
                {
                    SetLaserColor(Color.red);
                    // Взрываемся
                    explosive.Explode();
                    exploded = true;
                }
                else
                {
                    SetLaserColor(Color.white);
                }
            }
            else
            {
                // Рисуем лазер на всю длину
                DrawLaser(origin, origin + direction * laserDistance);
                SetLaserColor(Color.white);
            }
        }

        void DrawLaser(Vector3 start, Vector3 end)
        {
            if (laserLine != null)
            {
                laserLine.SetPosition(0, start);
                laserLine.SetPosition(1, end);
            }
        }

        void SetLaserColor(Color color)
        {
            if (laserLine != null)
            {
                laserLine.startColor = color;
                laserLine.endColor = color;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * laserDistance);
        }
    }
}