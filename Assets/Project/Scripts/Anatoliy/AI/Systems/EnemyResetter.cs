using Akila.FPSFramework;
using UnityEngine;

public class EnemyResetter : MonoBehaviour
{
    private void Start()
    {
        SpawnManager.Instance.onPlayerSpwanWithObjName.AddListener(ResetAllEnemies);
    }

    public void ResetAllEnemies(string name)
    {
        var newEnemies = transform.GetComponentsInChildren<EnemyBase>(true);
        foreach (var enemy in newEnemies)
        {
            if (enemy == null) continue;

            if (!enemy.IsSpawnedBySpawner)
                enemy.FullReset();
            else
                Destroy(enemy.gameObject);
        }

        // Legacy
        var legacyEnemies = transform.GetComponentsInChildren<EnemyAI>(true);
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
