using UnityEngine;

public class DifficultySelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _difficultyPanel;
    [SerializeField] private GameObject _mainMenuPanel;

    private const string DifficultyPrefsKey = "GameDifficulty";

    public void SelectEasy()
    {
        DifficultyManager.Instance.SetEasyDifficulty();
    }

    public void SelectHard()
    {
        DifficultyManager.Instance.SetHardDifficulty();
    }

    [ContextMenu("Debug/Reset Difficulty Choice")]
    private void ResetDifficultyChoice()
    {
        PlayerPrefs.DeleteKey(DifficultyPrefsKey);
        PlayerPrefs.Save();
        Debug.Log("Difficulty choice reset. Screen will show on next launch.");
    }
}
