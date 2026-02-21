using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance => _instance;

    [SerializeField] private bool _isPlayAwake = true;
    [SerializeField] private AudioSource _ambiemtSource;
    [Header("AmbientInLocation")]
    [SerializeField] private AudioClip _audioClipInTransition;
    [SerializeField] private float _maxVolumeTransition;
    [Space]
    [SerializeField] private List<AudioClipInLocation> _audioAmbientInIdLocation = new();
    [SerializeField] private float _speedDown = 2f;
    [SerializeField] private float _speedUp = 2f;
    private static Dictionary<int, AudioClipInLocation> _ambientInIdLocationAudioClip;

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
        SceneLoader.instance.LevelLoaded -= OnLevelLoaded;

        _isDestroy = true;
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _isDestroy = false;

        InitAmbientInLocation();
        InitRandomAudioClip();
        InitScriptedAudioClip();

        if (_isPlayAwake)
            StartCoroutine(SetAmbientClip(_ambiemtSource, 0, false));
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
            _ambientInIdLocationAudioClip.Add(locationAudioClip.IdLocation, locationAudioClip);

        SceneLoader.instance.LevelLoaded += OnLevelLoaded;
    }

    public IEnumerator SetAmbientClip(AudioSource audioSource, int idLocation, bool isTransition)
    {
        if (audioSource == null || (idLocation != -1 && !_ambientInIdLocationAudioClip.ContainsKey(idLocation)))
        {
            Debug.LogWarning("Audio source is null!");
        }
        else
        {
            _isDestroy = false;

            if (audioSource.isPlaying)
                yield return VolumeDown(audioSource);

            if (isTransition && _audioClipInTransition != null)
                audioSource.clip = _audioClipInTransition;
            else if (!isTransition)
                audioSource.clip = _ambientInIdLocationAudioClip[idLocation].AudioClip;
            else
                goto exit;

            audioSource.Play();

            yield return VolumeUp(audioSource, !isTransition ? _ambientInIdLocationAudioClip[idLocation].MaxVolume : _maxVolumeTransition);
        }

    exit:;
    }

    public void TransitionIn()
    {
        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, -1, true));
    }

    public void TransitionOut()
    {
        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, SceneLoader.instance.CurrentSceneId, false));
    }

    private void OnLevelLoaded()
    {
        int currentLocationId = SceneLoader.instance.CurrentSceneId;
        bool isTransition = SceneLoader.instance.CheckCurrentSceneTransition;

        StopAllCoroutines();

        StartCoroutine(SetAmbientClip(_ambiemtSource, currentLocationId, isTransition));
    }

    private IEnumerator VolumeUp(AudioSource audioSource, float border)
    {
        while(audioSource.volume < 1 && audioSource.volume < border)
        {
            audioSource.volume += Time.unscaledDeltaTime * SpeedUp;

            if(audioSource.volume > border)
                audioSource.volume = border;

            yield return null;
        }
    }

    private IEnumerator VolumeDown(AudioSource audioSource)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.unscaledDeltaTime * SpeedDown;

            if (audioSource.volume < 0)
                audioSource.volume = 0;

            yield return null;
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
    public float MaxVolume;
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