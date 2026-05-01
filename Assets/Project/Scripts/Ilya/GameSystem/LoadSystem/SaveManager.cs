using Akila.FPSFramework;
using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    public static SaveManager Instance => _instance;

    [SerializeField] private static string _sceneNameKey = "LastScene";
    [Space]
    [SerializeField] private static string _difficultyKey = "GameDifficulty";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void SetLastSceneName(string name, bool isDebug = false)
    {
        if (isDebug)
            Debug.Log("Save scene: " + name);

        PlayerPrefs.SetString(_sceneNameKey, name);
    }

    public static string GetLastSceneName()
    {
        return PlayerPrefs.GetString(_sceneNameKey);
    }

    public static void SaveWeaponPlayer(Actor player, bool isDebug = false)
    {
        try
        {
            Inventory inventory = player.GetComponentInChildren<Inventory>();

            foreach (var col in inventory.collectables)
            {
                PlayerPrefs.SetInt(col.identifier.displayName, col.count);

                if (isDebug)
                    Debug.Log("Save Ammo: " + col.identifier.displayName + "Count - " + col.count);
            }

            Firearm[] weapons = player.GetComponentsInChildren<Firearm>(true);

            for (int i = 0; i < weapons.Length; i++)
            {
                PlayerPrefs.SetString("Weapon" + i, weapons[i].Name);
                int countAmmo = PlayerPrefs.GetInt(weapons[i].ammoProfile.identifier.displayName) + weapons[i].remainingAmmoCount;
                PlayerPrefs.SetInt(weapons[i].ammoProfile.identifier.displayName, countAmmo);

                if (isDebug)
                {
                    Debug.Log("Save Ammo: " + weapons[i].ammoProfile.identifier.displayName + " - " + countAmmo);
                    Debug.Log("Save Weapon" + i.ToString() + " " + weapons[i].Name);
                }
            }
        }
        catch (NullReferenceException)
        {
            if (isDebug)
                Debug.Log("Weapon not found || Inventory not found");
        }
    }

    public static void SetDifficulty(GameDifficulty difficulty)
    {
        PlayerPrefs.SetInt(_difficultyKey, (int)difficulty);
    }

    public static GameDifficulty GetDifficulty()
    {
        return (GameDifficulty)PlayerPrefs.GetInt(_difficultyKey);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void DeleteSave(bool isDebug = false)
    {
        if (isDebug)
            Debug.Log("Delete save");

        PlayerPrefs.DeleteAll();
    }
}
