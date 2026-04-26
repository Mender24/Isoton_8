using Akila.FPSFramework;
using UnityEngine;

public class Earthquake : MonoBehaviour
{
    [SerializeField] private float _cameraShake = 0.5f;
    [SerializeField] private string _soundEarthquakeName = "";

    public void ShakeCamera()
    {
        if(Player.Instance == null)
        {
            Debug.LogWarning("Player is null!");
            return;
        }

        Player.Instance.ShakeCamera(_cameraShake);
        SoundManager.Instance.PlayScriptedSoundName(_soundEarthquakeName);
    }
}
