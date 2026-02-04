using UnityEngine;
using System.Collections;

public class EnemyResetter : MonoBehaviour
{
    public void ResetAllEnemies()
    {
        var enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.spawnType == EnemyAI.SpawnSource.Manually)
            {
                enemy.FullReset();
            }
            else
            {
                Destroy(enemy.gameObject);
            }
        }
    }
}