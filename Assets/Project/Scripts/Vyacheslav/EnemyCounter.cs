using UnityEngine;
using System.Collections.Generic;
using Akila.FPSFramework;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter Instance;
    private List<IDamageable> _enemies;
    public void Register(IDamageable enemy)
    {
        _enemies.Add(enemy);
    }

    public List<IDamageable> GetEnemyBySphere(float radius, Vector3 center)
    {
        List<IDamageable> res = new();
        foreach (var enemy in _enemies)
        {
            if (enemy != null && enemy.IsSphereCollision(center, radius))
            {
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
