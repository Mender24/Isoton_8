using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    private const string PrefsKey = "GameDifficulty";

    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultySettings _settings;

    private DifficultyLevel _currentLevel;

    public static GameDifficulty CurrentDifficulty { get; private set; }

    public static float PlayerHealthMult => Instance != null ? Instance._currentLevel.playerHealthMult : 1f;
    public static float PlayerRegenMult => Instance != null ? Instance._currentLevel.playerRegenMult : 1f;
    public static float EnemyHealthMult => Instance != null ? Instance._currentLevel.enemyHealthMult : 1f;
    public static float EnemyDamageMult => Instance != null ? Instance._currentLevel.enemyDamageMult : 1f;
    public static float WeaponDamageMult => Instance != null ? Instance._currentLevel.weaponDamageMult : 1f;
    public static float AmmoDropChanceMult => Instance != null ? Instance._currentLevel.ammoDropChanceMult : 1f;
    public static float AmmoBoxCountMult => Instance != null ? Instance._currentLevel.ammoBoxCountMult : 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CurrentDifficulty = (GameDifficulty)PlayerPrefs.GetInt(PrefsKey, (int)GameDifficulty.Easy);
        _currentLevel = _settings.GetLevel(CurrentDifficulty);
        ApplyWeaponDamageModifier();
    }

    public static void SetDifficulty(GameDifficulty difficulty)
    {
        CurrentDifficulty = difficulty;

        SaveManager.SetDifficulty(difficulty);

        if (Instance != null)
        {
            Instance._currentLevel = Instance._settings.GetLevel(difficulty);
            ApplyWeaponDamageModifier();
        }
    }

    public static void LoadDifficulty()
    {
        Debug.Log(SaveManager.GetDifficulty());
        SetDifficulty(SaveManager.GetDifficulty());
    }

    public void SetEasyDifficulty()
    {
        SetDifficulty(GameDifficulty.Easy);
    }

    public void SetHardDifficulty()
    {
        SetDifficulty(GameDifficulty.Hard);
    }

    private static void ApplyWeaponDamageModifier()
    {
        Projectile.DamageModifier = dmg => dmg * WeaponDamageMult;
    }
}
