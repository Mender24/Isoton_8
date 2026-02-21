using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private bool _isPlayAwake = true;
    [SerializeField] private AudioSource _ambiemtSource;
    [Header("AmbientInLocation")]
    [SerializeField] private AudioClip _audioClipInTransition;
    [SerializeField] private List<AudioClipInLocation> _audioAmbientInIdLocation = new();
    [SerializeField] private float _speedDown = 2f;
    [SerializeField] private float _speedUp = 2f;
    private static Dictionary<int, AudioClip> _ambientInIdLocationAudioClip;

    public static float SpeedDown { get; private set; }
    public static float SpeedUp { get; private set; }

    [Space]
    [Header("RandomAudioClip")]
    [SerializeField] private List<ProfileRandomAudioClip> _randomAudioClip = new();
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _percentageOccurence = 0.2f;
    [SerializeField] private float _timeBetween = 5f;
    [Space]
    [Header("ScriptedAudioClip")]
    [SerializeField] private List<ScriptedAudioClip> _scriptedAudios = new();
    private static Dictionary<string, AudioClip> _scriptedAudioClipInName;

    private static bool _isDestroy = false;

    private void OnDestroy()
    {
        _isDestroy = true;
    }

    private void Awake()
    {
        _isDestroy = false;

        InitAmbientInLocation();
        InitRandomAudioClip();
        InitScriptedAudioClip();

        if (_isPlayAwake)
            SetAmbientClip(_ambiemtSource, 0, false);
    }

    #region AmbientInLocation

    private void InitAmbientInLocation()
    {
        if (_ambiemtSource != null)
            _ambiemtSource.volume = 0;

        SpeedDown = _speedDown;
        SpeedUp = _speedUp;

        _ambientInIdLocationAudioClip = new();

        foreach(AudioClipInLocation locationAudioClip in _audioAmbientInIdLocation)
            _ambientInIdLocationAudioClip.Add(locationAudioClip.IdLocation, locationAudioClip.AudioClip);
    }

    public static async void SetAmbientClip(AudioSource audioSource, int idLocation, bool isTransition)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Audio source is null!");
            return;
        }

        if(audioSource.isPlaying)
            await VolumeDown(audioSource);

        if (!_ambientInIdLocationAudioClip.ContainsKey(idLocation))
            return;

        if (isTransition)
            idLocation++;

        Debug.Log(_ambientInIdLocationAudioClip[idLocation]);
        audioSource.clip = _ambientInIdLocationAudioClip[idLocation];
        audioSource.Play();

        await VolumeUp(audioSource);
    }

    private static async Task VolumeUp(AudioSource audioSource)
    {
        while(audioSource.volume < 1)
        {
            if (_isDestroy)
                break;

            audioSource.volume += Time.unscaledDeltaTime * SpeedUp;

            await Task.Yield();
        }
    }

    private static async Task VolumeDown(AudioSource audioSource)
    {
        while (audioSource.volume > 0)
        {
            if (_isDestroy)
                break;

            audioSource.volume -= Time.unscaledDeltaTime * SpeedDown;

            await Task.Yield();
        }
    }

    #endregion

    #region RandomAudioClip

    private void InitRandomAudioClip()
    {

    }

    #endregion

    #region ScriptedAudioClip

    private void InitScriptedAudioClip()
    {
        _scriptedAudioClipInName = new();

        foreach (ScriptedAudioClip clip in _scriptedAudios)
            _scriptedAudioClipInName.Add(clip.NameAudioClip, clip.AudioClip);
    }

    public static void PlayScriptedSoundName(AudioSource audioSource, string name)
    {
        audioSource.clip = _scriptedAudioClipInName[name];
        audioSource.Play();
    }

    #endregion
}

[System.Serializable]
public class AudioClipInLocation
{
    public AudioClip AudioClip;
    public int IdLocation;
}

[System.Serializable]
public class ProfileRandomAudioClip
{
    public int LocationId;
    public List<AudioClip> RandomAudioClip;
}

[System.Serializable]
public class ScriptedAudioClip
{
    public string NameAudioClip;
    public AudioClip AudioClip;
}