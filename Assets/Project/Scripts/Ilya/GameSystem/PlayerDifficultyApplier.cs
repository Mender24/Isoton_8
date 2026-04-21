using Akila.FPSFramework;
using UnityEngine;

/// Applies difficulty multipliers to player's Damageable on scene start.
/// Add this component to the player prefab alongside Damageable.
public class PlayerDifficultyApplier : MonoBehaviour
{
    private void Start()
    {
        var damageable = GetComponent<Damageable>();
        if (damageable == null) return;

        damageable.health    *= DifficultyManager.PlayerHealthMult;
        damageable.maxHealth  = damageable.health;
        damageable.regenerationRate *= DifficultyManager.PlayerRegenMult;
    }
}
