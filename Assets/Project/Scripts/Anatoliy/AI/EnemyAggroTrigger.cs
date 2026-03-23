using UnityEngine;
using Akila.FPSFramework;

/// <summary>
/// Триггер, который немедленно агрит ботов на игрока при входе в зону.
/// Враги агрятся без задержки обнаружения, минуя визомер.
/// </summary>
public class EnemyAggroTrigger : MonoBehaviour
{
    [Tooltip("Список врагов для агра. Если пусто ищет EnemyBase у детей родителя.")]
    public EnemyBase[] enemies;

    [Tooltip("Сработать только один раз")]
    public bool activateOnce = true;

    private bool _triggered;

    private void Awake()
    {
        if (enemies == null || enemies.Length == 0)
        {
            Transform root = transform.parent != null ? transform.parent : transform;
            enemies = root.GetComponentsInChildren<EnemyBase>();
        }
    }

    private void Start()
    {
        if (enemies.Length == 0)
            Debug.LogWarning($"[EnemyAggroTrigger] '{gameObject.name}': враги не найдены. Добавьте врагов вручную или разместите их дочерними объектами.", this);

        if (activateOnce && SpawnManager.Instance != null)
            SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(_ => _triggered = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _triggered) return;
        if (!other.CompareTag("Player")) return;

        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy.AggroOnPlayer();
        }

        _triggered = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
        }
    }
#endif
}
