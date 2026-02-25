using UnityEngine;

[CreateAssetMenu(fileName = "MeleeEnemyConfig", menuName = "Enemy/Melee Config")]
public class MeleeEnemyConfig : ScriptableObject
{
    [Header("Attack")]
    public float AttackRange = 2f;
    public float Damage = 20f;
    public float AttackCooldown = 1.5f;
    public float AttackDuration = 3.0f;

    [Header("Hit Detection")]
    public LayerMask HitLayers;
    public Vector3 AttackOffset = new Vector3(0, 1f, 1f);
    public float AttackRadius = 1f;
}
