using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    public static SaveManager Instance => _instance;

    [SerializeField] private string _sceneNameSave = "LastScene";

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

    public void SetLastSceneName(string name)
    {
        PlayerPrefs.SetString(_sceneNameSave, name);
    }

    public string GetLastSceneName()
    {
        return PlayerPrefs.GetString(_sceneNameSave, "");
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}
