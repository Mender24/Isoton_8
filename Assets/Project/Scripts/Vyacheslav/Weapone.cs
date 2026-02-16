using UnityEngine;

public class Weapone : Akila.FPSFramework.Firearm
{
    protected override void DoFireDone(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        base.DoFireDone(position, rotation, direction);
        DoShoot(position, rotation, direction);
    }

    private void DoShoot(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        LightingProjectile projectile = PoolManager.Instance.GetObject<LightingProjectile>();
    }
}
