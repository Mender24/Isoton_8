using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.Video;

public class TriggerOutroVideo : MonoBehaviour
{
    [SerializeField] private CutscenePlayer _outroCutscenePlayer;
    [SerializeField] private VideoClip _outroClip;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.gameObject.TryGetComponent(out Player _)) return;

        _triggered = true;
        FPSFrameworkCore.IsInputActive = false;

        if (_outroCutscenePlayer != null && _outroClip != null)
            _outroCutscenePlayer.PlayCutscene(_outroClip, () => SceneLoader.instance.LoadMainMenu());
        else
            SceneLoader.instance.LoadMainMenu();
    }
}
