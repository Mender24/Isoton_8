using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RawImage _videoDisplay;

    [Header("Fade")]
    [SerializeField] private CanvasGroup _fadeOverlay;
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Skip UI")]
    [SerializeField] private CanvasGroup _skipHintGroup;
    [SerializeField] private Image _skipProgressBar;
    [SerializeField] private KeyCode _skipKey = KeyCode.Space;

    [Header("Settings")]
    [SerializeField] private float _skipHoldDuration = 2f;
    [SerializeField] private float _skipDecaySpeed = 1f;
    [SerializeField] private float _hintShowDuration = 2.5f;
    [SerializeField] private float _hintFadeDuration = 0.4f;
    [SerializeField] private bool _unlockCursorOnPlay = false;

    [Header("Hide During Playback")]
    [SerializeField] private GameObject[] _hideOnPlay;
    [SerializeField] private AudioSource[] _muteOnPlay;

    private UnityAction _onComplete;
    private RenderTexture _renderTexture;
    private Coroutine _hintFadeCoroutine;
    private float[] _savedVolumes;

    private float _holdTimer;
    private float _hintHideTimer;
    private bool _isPlaying;
    private bool _hintVisible;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void PlayCutscene(VideoClip clip, UnityAction onComplete)
    {
        _onComplete = onComplete;
        _isPlaying = false;

        _holdTimer = 0f;
        _skipProgressBar.fillAmount = 0f;
        _skipHintGroup.alpha = 0f;
        _hintVisible = false;

        if (_fadeOverlay != null)
            _fadeOverlay.alpha = 1f;

        gameObject.SetActive(true);

        foreach (GameObject obj in _hideOnPlay)
            if (obj != null) obj.SetActive(false);

        _savedVolumes = new float[_muteOnPlay.Length];
        for (int i = 0; i < _muteOnPlay.Length; i++)
        {
            if (_muteOnPlay[i] == null) continue;
            _savedVolumes[i] = _muteOnPlay[i].volume;
            _muteOnPlay[i].volume = 0f;
        }

        if (_unlockCursorOnPlay)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        CreateRenderTexture();

        _videoPlayer.clip = clip;
        _videoPlayer.loopPointReached += OnVideoEnd;
        _videoPlayer.prepareCompleted += OnVideoPrepared;
        _videoPlayer.Prepare();
    }

    private void CreateRenderTexture()
    {
        if (_renderTexture != null && _renderTexture.width == Screen.width && _renderTexture.height == Screen.height)
            return;

        if (_renderTexture != null)
            _renderTexture.Release();

        _renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        _videoPlayer.targetTexture = _renderTexture;
        _videoDisplay.texture = _renderTexture;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        vp.Play();
        _isPlaying = true;

        if (_fadeOverlay != null)
            StartCoroutine(FadeOverlay(1f, 0f));
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        CompleteCutscene();
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (Input.anyKeyDown && !Input.GetKeyDown(_skipKey))
            ShowSkipHint();

        if (Input.GetKey(_skipKey))
        {
            ShowSkipHint();
            _holdTimer += Time.unscaledDeltaTime;
            _skipProgressBar.fillAmount = _holdTimer / _skipHoldDuration;

            if (_holdTimer >= _skipHoldDuration)
                CompleteCutscene();
        }
        else if (_holdTimer > 0f)
        {
            _holdTimer -= Time.unscaledDeltaTime * _skipDecaySpeed;
            _holdTimer = Mathf.Max(0f, _holdTimer);
            _skipProgressBar.fillAmount = _holdTimer / _skipHoldDuration;
        }

        if (_hintVisible && !Input.GetKey(_skipKey))
        {
            _hintHideTimer -= Time.unscaledDeltaTime;
            if (_hintHideTimer <= 0f)
                HideSkipHint();
        }
    }

    private void ShowSkipHint()
    {
        _hintHideTimer = _hintShowDuration;
        if (_hintVisible) return;

        _hintVisible = true;
        if (_hintFadeCoroutine != null) StopCoroutine(_hintFadeCoroutine);
        _hintFadeCoroutine = StartCoroutine(FadeHint(1f));
    }

    private void HideSkipHint()
    {
        _hintVisible = false;
        if (_hintFadeCoroutine != null) StopCoroutine(_hintFadeCoroutine);
        _hintFadeCoroutine = StartCoroutine(FadeHint(0f));
    }

    private IEnumerator FadeHint(float target)
    {
        float start = _skipHintGroup.alpha;
        float elapsed = 0f;

        while (elapsed < _hintFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _skipHintGroup.alpha = Mathf.Lerp(start, target, elapsed / _hintFadeDuration);
            yield return null;
        }

        _skipHintGroup.alpha = target;
    }

    private void CompleteCutscene()
    {
        if (!_isPlaying) return;
        _isPlaying = false;

        _videoPlayer.loopPointReached -= OnVideoEnd;
        _videoPlayer.Stop();

        StartCoroutine(FadeOutAndComplete());
    }

    private IEnumerator FadeOutAndComplete()
    {
        if (_fadeOverlay != null)
            yield return StartCoroutine(FadeOverlay(0f, 1f));

        gameObject.SetActive(false);

        foreach (GameObject obj in _hideOnPlay)
            if (obj != null) obj.SetActive(true);

        for (int i = 0; i < _muteOnPlay.Length; i++)
            if (_muteOnPlay[i] != null) _muteOnPlay[i].volume = _savedVolumes[i];

        UnityAction callback = _onComplete;
        _onComplete = null;
        callback?.Invoke();
    }

    private IEnumerator FadeOverlay(float from, float to)
    {
        _fadeOverlay.alpha = from;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _fadeOverlay.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            yield return null;
        }

        _fadeOverlay.alpha = to;
    }

    private void OnDestroy()
    {
        if (_renderTexture != null)
            _renderTexture.Release();
    }
}
