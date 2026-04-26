using Akila.FPSFramework;
using UnityEngine;

public class Earthquake : MonoBehaviour
{
    [SerializeField] private float _cameraShakeMultiplier = 0.5f;
    [SerializeField] private float _roughness = 1;
    [Range(0, 100)]
    [SerializeField] private float _fadeInTime = 0.01f;
    [Range(0, 10)]
    [SerializeField] private float _fadeOutTime = 2f;
    [Space]
    [SerializeField] private string _soundEarthquakeName = "";
    [Space]
    [SerializeField] private bool _isTest = false;

    //float multiplier, float roughness, float fadeInTime, float fadeOutTime
    private void Update()
    {
        if(_isTest)
        {
            _isTest = false;
            ShakeCamera();
        }
    }

    public void ShakeCamera()
    {
        if(Player.Instance == null)
        {
            Debug.LogWarning("Player is null!");
            return;
        }

        Player.Instance.ShakeCamera(_cameraShakeMultiplier, _roughness, _fadeInTime, _fadeOutTime);
        SoundManager.Instance.PlayScriptedSoundName(_soundEarthquakeName);
    }
}
