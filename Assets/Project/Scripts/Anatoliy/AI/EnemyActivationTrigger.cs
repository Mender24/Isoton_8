using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    public EnemyAI[] enemiesInZone;
    public bool activateOnce = true;
    private bool _hasActivated = false;

    void Start()
    {
        if (enemiesInZone.Length == 0)
        {
            Debug.LogError("No enemies were assigned to the trigger " + gameObject.name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _hasActivated) return;
        
        if (other.CompareTag("Player"))
        {
            foreach (var enemy in enemiesInZone)
            {
                if (enemy != null)
                {
                    enemy.isActivated = true;
                    enemy.playerTransform = other.transform;
                }
            }
            
            _hasActivated = true;
        }
    }
}