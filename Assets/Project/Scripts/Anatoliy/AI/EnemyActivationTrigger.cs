using Akila.FPSFramework;
using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    public enum Mode
    {
        Default,        // просто IsActivated = true
        WithBehavior,   // включить BehaviorAgent + активировать
        Alerted         // включить BehaviorAgent + сразу в боевое состояние
    }

    public EnemyBase[] enemiesInZone;
    public Mode activationMode = Mode.Default;
    public bool activateOnce = true;
    private bool _hasActivated = false;

    public EnemyAI[] enemiesInZone_legacy;

    void Awake()
    {
        // Auto-collect from parent's children (siblings + their children) if not assigned manually
        Transform searchRoot = transform.parent != null ? transform.parent : transform;

        if (enemiesInZone.Length == 0)
            enemiesInZone = searchRoot.GetComponentsInChildren<EnemyBase>();

        if (enemiesInZone_legacy.Length == 0)
            enemiesInZone_legacy = searchRoot.GetComponentsInChildren<EnemyAI>();
    }

    void Start()
    {
        if (enemiesInZone.Length == 0 && enemiesInZone_legacy.Length == 0)
            Debug.LogWarning("No enemies found for trigger " + gameObject.name + ". Add enemies as children or siblings under a parent object.");

        if (activateOnce && SpawnManager.Instance != null)
            SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(OnPlayerRespawned);
    }

    private void OnPlayerRespawned(string _) => _hasActivated = false;

    void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _hasActivated) return;
        
        if (other.CompareTag("Player"))
        {
            foreach (var enemy in enemiesInZone_legacy)
            {
                if (enemy != null)
                {
                    enemy.isActivated = true;
                    enemy.playerTransform = other.transform;
                    Debug.Log("Activated legacy enemy");
                }
            }

            foreach (var enemy in enemiesInZone)
            {
                if (enemy == null) continue;
                switch (activationMode)
                {
                    case Mode.WithBehavior: enemy.ActivateWithBehavior(); break;
                    case Mode.Alerted:      enemy.ActivateAlerted();      break;
                    default:               enemy.Activate();              break;
                }
            }            
            
            _hasActivated = true;
        }
    }
}