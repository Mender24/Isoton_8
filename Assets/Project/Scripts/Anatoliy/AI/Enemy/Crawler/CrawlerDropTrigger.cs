using UnityEngine;
using Akila.FPSFramework;

/// <summary>
/// Триггер, который приказывает ScriptedCeilingCrawler прыгнуть с потолка к игроку.
/// Поместите этот компонент на объект с Collider (Is Trigger = true).
/// Автоматически находит ближайшего ScriptedCeilingCrawler в дочерних / родительских объектах,
/// если массив _crawlers не задан вручную.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CrawlerDropTrigger : MonoBehaviour
{
    [Tooltip("Краулеры, которые сбросятся при срабатывании. Если пусто — ищет автоматически.")]
    [SerializeField] private ScriptedCeilingCrawler[] _crawlers;

    [Tooltip("Сработать только один раз")]
    [SerializeField] private bool _triggerOnce = true;

    private bool _triggered;

    private void Awake()
    {
        if (_crawlers == null || _crawlers.Length == 0)
        {
            Transform root = transform.parent != null ? transform.parent : transform;
            _crawlers = root.GetComponentsInChildren<ScriptedCeilingCrawler>();
        }

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Start()
    {
        if ((_crawlers == null || _crawlers.Length == 0))
            Debug.LogWarning($"[CrawlerDropTrigger] {gameObject.name}: краулеры не найдены. " +
                             "Добавьте их вручную или разместите триггер как дочерний/сестринский объект краулера.");

        // Сброс при респауне игрока
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(_ => _triggered = false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggerOnce && _triggered) return;
        if (!other.CompareTag("Player"))          return;

        _triggered = true;

        foreach (var crawler in _crawlers)
        {
            if (crawler != null)
                crawler.TriggerDrop();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        var col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
#endif
}
