using UnityEngine;
using UnityEngine.Video;

public class ButtonLoadGame : MonoBehaviour
{
    [SerializeField] private bool _isNewGame = false;
    [SerializeField] private string _forceSceneLoad;

    [Header("Intro Cutscene")]
    [SerializeField] private CutscenePlayer _introCutscenePlayer;
    [SerializeField] private VideoClip _introClip;

    private bool _isActive = true;

    public void LoadGame()
    {
        if (!_isActive) return;
        _isActive = false;

        if (!_isNewGame)
            DifficultyManager.LoadDifficulty();

        // StartLoadGame();

        // return;

        if (_isNewGame && _introCutscenePlayer != null && _introClip != null)
        {
            // SceneLoader.instance.PreloadFirstScene(_forceSceneLoad, !_isNewGame);
            _introCutscenePlayer.PlayCutscene(_introClip, StartLoadGame);
        }
        else
            StartLoadGame();
    }

    private void StartLoadGame()
    {
        SceneLoader.instance.LoadScenes(true, _forceSceneLoad, !_isNewGame);
    }

    public void LoadMeinMenu()
    {
        if (_isActive)
        {
            _isActive = false;
            SceneLoader.instance.LoadMainMenu();
        }
    }

    public void ExitGame()
    {
        if (_isActive)
        {
            _isActive = false;
            SceneLoader.instance.ExitGame();
        }
    }
}
