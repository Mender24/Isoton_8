using UnityEngine;

public class Weapone : Akila.FPSFramework.Firearm
{
    protected override void DoSpecialShoot(Vector3 position, Quaternion rotation, Vector3 direction, float speed, float range)
    {
        LightingProjectile projectile = PoolManager.Instance.GetObject<LightingProjectile>();
        projectile.transform.position = muzzle.transform.position;
        projectile.gameObject.SetActive(true);
        projectile.Setup(direction, projectile.LifeTime, projectile.speed);
        Debug.LogError("DoShoot pos" + position + " my pos " + transform.position);
    }
}
