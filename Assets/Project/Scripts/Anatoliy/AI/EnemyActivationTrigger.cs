using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    public EnemyBase[] enemiesInZone;
    public bool activateOnce = true;
    private bool _hasActivated = false;

    public EnemyAI[] enemiesInZone_legacy;

    void Start()
    {
        if (enemiesInZone.Length == 0)
        {
            Debug.LogError("No enemies were assigned to the trigger " + gameObject.name);
        }

        if (enemiesInZone_legacy.Length == 0)
        {
            Debug.LogError("No enemies were assigned to the trigger LEGACY " + gameObject.name);
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
                    enemy.State.IsActivated = true;
                    // enemy.PlayerTransform = other.transform;
                }
            }            
            
            _hasActivated = true;
        }
    }
}