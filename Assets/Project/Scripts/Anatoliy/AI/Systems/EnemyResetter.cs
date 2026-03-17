using UnityEngine;

public class EnemyResetter : MonoBehaviour
{
    public void ResetAllEnemies()
    {
        var newEnemies = FindObjectsByType<EnemyBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var enemy in newEnemies)
        {
            if (enemy == null) continue;

            if (!enemy.IsSpawnedBySpawner)
                enemy.FullReset();
            else
                Destroy(enemy.gameObject);
        }

        // Legacy
        var legacyEnemies = FindObjectsByType<EnemyAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var enemy in legacyEnemies)
        {
            if (enemy == null) continue;

            if (enemy.spawnType == EnemyAI.SpawnSource.Manually)
                enemy.FullReset();
            else
                Destroy(enemy.gameObject);
        }
    }
}
