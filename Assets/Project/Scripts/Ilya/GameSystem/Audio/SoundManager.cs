using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("AmbientInLocation")]
    [SerializeField] private AudioClip _audioClipInTransition;
    [SerializeField] private List<AudioClipInLocation> _audioAmbientInIdLocation = new();
    private static Dictionary<int, AudioClip> _ambientInIdLocationAudioClip;
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

    private void Awake()
    {
        InitAmbientInLocation();
        InitRandomAudioClip();
        InitScriptedAudioClip();
    }

    #region AmbientInLocation

    private void InitAmbientInLocation()
    {
        _ambientInIdLocationAudioClip = new();

        foreach(AudioClipInLocation locationAudioClip in _audioAmbientInIdLocation)
            _ambientInIdLocationAudioClip.Add(locationAudioClip.IdLocation, locationAudioClip.AudioClip);
    }

    public static void SetAmbientClip(AudioSource audioSource, int idLocation, bool isTransition)
    {
        if (!_ambientInIdLocationAudioClip.ContainsKey(idLocation))
            return;

        if (isTransition)
            idLocation++;

        audioSource.clip = _ambientInIdLocationAudioClip[idLocation];
        audioSource.Play();
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

    public static void PlayScriptedSoundName(string name, AudioSource audioSource)
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