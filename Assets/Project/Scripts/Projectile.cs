using UnityEngine;
using Akila.FPSFramework;
using System;

public class Projectile : Akila.FPSFramework.Projectile
{

    [SerializeField] private OnHitProjectileEffect _onHitProjectileEffect;
    [SerializeField] private GameObject _effectPrefab;
    [SerializeField] private GameObject _freezeTimeZone;
    public override void OnHit(RaycastHit hit, Ray ray)
    {
        if (!isActive) return;
        switch (_onHitProjectileEffect)
        {
            case OnHitProjectileEffect.None:
                OnHitEffectBase(hit, ray);
                break;
            case OnHitProjectileEffect.Explosive:
                OnHitEffectExplosive(hit);
                break;
            case OnHitProjectileEffect.SwapPlayer:
                OnHitEffectSwapPlayer(hit, ray);
                break;
            case OnHitProjectileEffect.SwapEnemy:
                OnHitEffectSwapEnemy(hit, ray);
                break;
            case OnHitProjectileEffect.FreezeTime:
                OnHitEffectFreezeTime(hit);
                break;

        }

    }


    private void OnHitEffectSwapPlayer(RaycastHit hit, Ray ray)
    {
        Firearm.UpdateHits(source.firearm, defaultDecal, ray, hit, CalculateDamage(), decalDirection, SwapPlayer);
        // player.transform.rotation = rot;
    }

    private void SwapPlayer(IDamageable damageable)
    {
        var player = Player.Instance;
        var pos = damageable.gameObject.transform.position;
        //var rot = damageable.transform.rotation;
        damageable.gameObject.transform.position = player.transform.position;
        damageable.transform.rotation = player.transform.rotation;
        player.gameObject.transform.position = pos;
    }

    private void OnHitEffectSwapEnemy(RaycastHit hit, Ray ray)
    {
        Firearm.UpdateHits(source.firearm, defaultDecal, ray, hit, CalculateDamage(), decalDirection, SwapEnemy);
        this.gameObject.SetActive(false);
    }

    private void SwapEnemy(IDamageable damageable)
    {
        damageable.gameObject.SetActive(false);
        Instantiate(_effectPrefab, damageable.transform.position, damageable.transform.rotation, damageable.transform.parent);
        this.gameObject.SetActive(false);
    }
    private void OnHitEffectFreezeTime(RaycastHit hit)
    {
        Instantiate(_effectPrefab, hit.transform.position, hit.transform.rotation, hit.transform.parent);
        this.gameObject.SetActive(false);
    }
}

public enum OnHitProjectileEffect
{
    None = 1,
    Explosive = 2,
    SwapPlayer = 3,
    SwapEnemy = 4,
    FreezeTime = 5
}
