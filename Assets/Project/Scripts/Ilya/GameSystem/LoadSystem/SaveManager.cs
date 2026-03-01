using Akila.FPSFramework;
using System;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    public static SaveManager Instance => _instance;

    [SerializeField] private static string _sceneNameSave = "LastScene";

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

        PlayerPrefs.SetString(_sceneNameSave, name);
    }

    public static string GetLastSceneName()
    {
        return PlayerPrefs.GetString(_sceneNameSave);
    }

    public static void SaveWeaponPlayer(Actor player, bool isDebug = false)
    {
        try
        {
            Firearm[] weapons = player.GetComponentsInChildren<Firearm>(true);

            for (int i = 0; i < weapons.Length; i++)
            {
                PlayerPrefs.SetString("Weapon" + i, weapons[i].Name);

                if (isDebug)
                    Debug.Log("Save Weapon" + i.ToString() + " " + weapons[i].Name);
            }
        }
        catch (NullReferenceException)
        {
            if (isDebug)
                Debug.Log("Weapon not found");
        }
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
