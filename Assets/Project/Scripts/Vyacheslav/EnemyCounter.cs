using UnityEngine;
using System.Collections.Generic;
using Akila.FPSFramework;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter Instance;
    private List<IDamageable> _enemies;
    private float _registerDelta = 0.1f;
    public void Register(IDamageable enemy)
    {
        _enemies.Add(enemy);
    }

    public List<IDamageable> GetEnemyBySphere(float radius, Vector3 center)
    {
        List<IDamageable> res = new();
        foreach (var enemy in _enemies)
        {
            if (enemy != null && Vector3.Distance(enemy.transform.position, center) < radius)
            {
                Debug.LogError(enemy.transform.gameObject.name + " " +Vector3.Distance(enemy.transform.position, center));
                res.Add(enemy);
            }
        }
        return res;
    }
        

    private void Awake()
    {
        Instance = this;
        _enemies = new();
    }
}
