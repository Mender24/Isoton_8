using UnityEngine;

public class ButtonLoadGame : MonoBehaviour
{
    public string sceneName;

    public void LoadGame()
    {
        SceneLoader.instance.LoadStartMenu(sceneName);
    }

    public void LoadNewGame()
    {
        SceneLoader.instance.NewGame();
    }
}
