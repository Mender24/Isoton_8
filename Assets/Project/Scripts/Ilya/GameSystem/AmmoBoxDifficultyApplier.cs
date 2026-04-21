using Akila.FPSFramework;
using UnityEngine;

/// Scales collectableCount on Pickable by difficulty multiplier.
/// Add this component to the ammo box prefab alongside Pickable.
public class AmmoBoxDifficultyApplier : MonoBehaviour
{
    private void Awake()
    {
        var pickable = GetComponent<Pickable>();
        if (pickable == null) return;

        pickable.collectableCount = Mathf.Max(1, Mathf.RoundToInt(pickable.collectableCount * DifficultyManager.AmmoBoxCountMult));
    }
}
