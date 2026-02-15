using UnityEngine;

public class ButtonLoadGame : MonoBehaviour
{
    public string sceneName;

    private bool _isActive = true;

    public void NextSceneLoad()
    {
        SceneLoader.instance.LoadNextScene();
    }

    public void LoadGame()
    {
        if(_isActive)
        {
            _isActive = false;
            SceneLoader.instance.LoadStartMenu(sceneName);
        }
    }

    public void LoadNewGame()
    {
        if(_isActive)
        {
            _isActive = false;
            SceneLoader.instance.NewGame();
        }
    }
}
