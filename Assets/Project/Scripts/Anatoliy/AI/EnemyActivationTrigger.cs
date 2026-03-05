using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    public EnemyBase[] enemiesInZone;
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
        {
            Debug.LogWarning("No enemies found for trigger " + gameObject.name + ". Add enemies as children or siblings under a parent object.");
        }
    }

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
                if (enemy != null)
                {
                    if (enemy.State)
                        enemy.State.IsActivated = true;
                    // enemy.PlayerTransform = other.transform;
                }
            }            
            
            _hasActivated = true;
        }
    }
}