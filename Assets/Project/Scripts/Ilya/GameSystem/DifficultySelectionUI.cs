using UnityEngine;

public class DifficultySelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _difficultyPanel;
    [SerializeField] private GameObject _mainMenuPanel;

    private const string DifficultyPrefsKey = "GameDifficulty";

    void Start()
    {
        if (PlayerPrefs.HasKey(DifficultyPrefsKey))
            OpenMainMenu();
        else
            OpenDifficultyPanel();
    }

    public void SelectEasy()
    {
        DifficultyManager.Instance.SetEasyDifficulty();
        OpenMainMenu();
    }

    public void SelectHard()
    {
        DifficultyManager.Instance.SetHardDifficulty();
        OpenMainMenu();
    }

    private void OpenMainMenu()
    {
        _difficultyPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    private void OpenDifficultyPanel()
    {
        _mainMenuPanel.SetActive(false);
        _difficultyPanel.SetActive(true);
    }

    [ContextMenu("Debug/Reset Difficulty Choice")]
    private void ResetDifficultyChoice()
    {
        PlayerPrefs.DeleteKey(DifficultyPrefsKey);
        PlayerPrefs.Save();
        Debug.Log("Difficulty choice reset. Screen will show on next launch.");
    }
}
