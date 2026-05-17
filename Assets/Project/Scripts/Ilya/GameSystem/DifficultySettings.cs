using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Game/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    public DifficultyLevel easy;
    public DifficultyLevel hard;

    public DifficultyLevel GetLevel(GameDifficulty difficulty)
    {
        return difficulty == GameDifficulty.Hard ? hard : easy;
    }
}

[System.Serializable]
public class DifficultyLevel
{
    [Header("Player")]
    public float playerHealthMult   = 1f;
    public float playerRegenMult    = 1f;

    [Header("Enemy")]
    public float enemyHealthMult    = 1f;
    public float enemyDamageMult    = 1f;

    [Header("Weapon (Player)")]
    public float weaponDamageMult   = 1f;

    [Header("Enemy Accuracy")]
    public float enemyChanceToHitMult = 1f;

    [Header("Ammo")]
    public float ammoDropChanceMult = 1f;
    public float ammoBoxCountMult   = 1f;
}

public enum GameDifficulty
{
    Easy = 0,
    Hard = 1
}
