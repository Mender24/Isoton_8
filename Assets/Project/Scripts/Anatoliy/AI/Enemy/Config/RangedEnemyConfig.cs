using UnityEngine;

[CreateAssetMenu(fileName = "RangedEnemyConfig", menuName = "Enemy/Ranged Config")]
public class RangedEnemyConfig : ScriptableObject
{
    [Header("Combat Range")]
    public float AttackRange = 20f;

    [Header("Shooting")]
    public GameObject BulletPrefab;
    public float Damage = 5f;
    public float BulletSpeed = 20f;
    public float BulletLifetime = 2f;
    public int MagazineSize = 20;
    public float FireRate = 0.5f;
    [Range(0f, 1f)]
    public float ChanceToHit = 0.9f;

    [Header("Reload")]
    public float ReloadTime = 3f;

    [Header("Aim Offset (shoot target)")]
    public float XOffset = 0f;
    public float YOffset = 1f;

    [Header("Spray")]
    public float HeightSprayOffset = 0.25f;
    public float WidthSprayOffset  = 0.25f;
}
