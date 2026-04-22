using UnityEngine;

public class DifficultySelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _difficultyPanel;
    [SerializeField] private GameObject _mainMenuPanel;

    void Start()
    {
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
}
