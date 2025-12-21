using Akila.FPSFramework;
using UnityEngine;

public class SwapFirearm : Akila.FPSFramework.Firearm
{
    [SerializeField] private GameObject _swapPrefab;
    [SerializeField] private bool _isPlayerSwap;
    protected override void ShootEffect(IDamageable damageable)
    {
        if (!damageable.IsSwaped())
        {
            return;
        }
        if (_isPlayerSwap)
        {
            var player = Player.Instance;
            var pos = damageable.gameObject.transform.position;
            //var rot = damageable.transform.rotation;
            damageable.gameObject.transform.position = player.transform.position;
            damageable.transform.rotation = player.transform.rotation;
            player.gameObject.transform.position = pos;
           // player.transform.rotation = rot;

        }
        else
        {
            damageable.gameObject.SetActive(false);
            Instantiate(_swapPrefab, damageable.transform.position, damageable.transform.rotation, damageable.transform.parent);
        }
        
    }
}
