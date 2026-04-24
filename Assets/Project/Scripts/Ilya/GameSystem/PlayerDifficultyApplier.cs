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

        float newHealth = damageable.health * DifficultyManager.PlayerHealthMult;
        float newRegenrationRate = damageable.regenerationRate * DifficultyManager.PlayerRegenMult;
        damageable.SetHealthSettings(newHealth, newRegenrationRate);
    }
}
