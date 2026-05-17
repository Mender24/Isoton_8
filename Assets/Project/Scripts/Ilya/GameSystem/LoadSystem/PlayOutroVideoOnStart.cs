using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayOutroVideoOnStart : MonoBehaviour
{
    [SerializeField] private CutscenePlayer _cutscenePlayer;
    [SerializeField] private VideoClip[] _clips;

    private bool _triggered;

    private void Start()
    {
        if (_triggered) return;
        _triggered = true;

        PlayNext(0);
    }

    private void PlayNext(int index)
    {
        if (_cutscenePlayer == null || _clips == null || index >= _clips.Length)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        VideoClip clip = _clips[index];

        if (clip == null)
        {
            PlayNext(index + 1);
            return;
        }

        _cutscenePlayer.PlayCutscene(clip, () => PlayNext(index + 1));
    }
}
