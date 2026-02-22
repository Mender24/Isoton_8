using UnityEngine;

public class ButtonLoadGame : MonoBehaviour
{
    [SerializeField] private bool _isNewGame = false;
    [SerializeField] private string _forceSceneLoad;

    private bool _isActive = true;

    public void LoadGame()
    {
        if(_isActive)
        {
            _isActive = false;
            SceneLoader.instance.LoadScenes(true, _forceSceneLoad, !_isNewGame);
        }
    }
}
