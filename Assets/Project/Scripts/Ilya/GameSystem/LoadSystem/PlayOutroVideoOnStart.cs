using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayOutroVideoOnStart : MonoBehaviour
{
    [SerializeField] private CutscenePlayer _outroCutscenePlayer;
    [SerializeField] private VideoClip _outroClip;
    
    private bool _triggered;
    void Start()
    {
        if (_triggered) return;
        _triggered = true;

        if (_outroCutscenePlayer != null && _outroClip != null)
            _outroCutscenePlayer.PlayCutscene(_outroClip, () => SceneManager.LoadScene("MainMenu"));
        else
            SceneLoader.instance.LoadMainMenu();
    }
}
