using UnityEngine;

[CreateAssetMenu(fileName = "EnemyGrenadeConfig", menuName = "Enemy/Grenade Config")]
public class EnemyGrenadeConfig : ScriptableObject
{
    [Header("Grenade Prefab")]
    public GameObject GrenadePrefab;

    [Header("Throw Decision")]
    [Range(0f, 1f)]
    public float ThrowChance = 0.5f;
    public int BulletsBeforeCheck = 20;

    [Header("Cooldown")]
    public float ThrowCooldown = 15f;

    [Header("Trajectory")]
    public float ThrowTime = 1.5f;
    public float LandingRandomRadius = 2f;

    [Header("Animation Timing")]
    public float WindUpDuration = 0.8f;
    public float ThrowDuration  = 0.6f;
}
