using UnityEngine;

/// <summary>
/// Animates a crystal with Z-axis rotation and multi-plane wave levitation.
/// Use the public API to control the animation from other scripts or UnityEvents.
/// </summary>
public class CrystalAnimator : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Levitation")]
    [SerializeField] private float verticalAmplitude = 0.15f;
    [SerializeField] private float verticalFrequency = 1.2f;

    [SerializeField] private float horizontalAmplitudeX = 0.06f;
    [SerializeField] private float horizontalFrequencyX = 0.8f;

    [SerializeField] private float horizontalAmplitudeZ = 0.04f;
    [SerializeField] private float horizontalFrequencyZ = 1.05f;

    [Header("Randomness")]
    [SerializeField] private float rotationSpeedVariance = 15f;
    [SerializeField] private float frequencyVariance = 0.3f;
    [SerializeField] private float amplitudeVariance = 0.05f;

    // ---- State ----
    private bool _rotationEnabled = true;
    private bool _levitationEnabled = true;

    private Vector3 _originLocalPosition;
    private float _time;

    // per-instance randomised values
    private float _rotSpeed;
    private float _freqY, _freqX, _freqZ;
    private float _ampY, _ampX, _ampZ;
    private float _phaseY, _phaseX, _phaseZ;

    /// <summary>Enable or disable Z rotation.</summary>
    public void SetRotation(bool enabled) => _rotationEnabled = enabled;

    /// <summary>Enable or disable wave levitation.</summary>
    public void SetLevitation(bool enabled)
    {
        _levitationEnabled = enabled;
        if (!enabled)
            transform.localPosition = _originLocalPosition;
    }

    /// <summary>Toggle rotation on/off.</summary>
    public void ToggleRotation() => SetRotation(!_rotationEnabled);

    /// <summary>Toggle levitation on/off.</summary>
    public void ToggleLevitation() => SetLevitation(!_levitationEnabled);

    /// <summary>Enable both rotation and levitation.</summary>
    public void EnableAll()
    {
        SetRotation(true);
        SetLevitation(true);
    }

    /// <summary>Disable both rotation and levitation.</summary>
    public void DisableAll()
    {
        SetRotation(false);
        SetLevitation(false);
    }

    /// <summary>
    /// Reset position, rotation and animation time back to origin.
    /// Does NOT change enabled/disabled state.
    /// </summary>
    public void ResetToOrigin()
    {
        _time = 0f;
        transform.localPosition = _originLocalPosition;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>Full reset: stop everything and return to origin.</summary>
    public void ResetFull()
    {
        DisableAll();
        ResetToOrigin();
    }

    private void Awake()
    {
        _originLocalPosition = transform.localPosition;

        _rotSpeed = rotationSpeed + Random.Range(-rotationSpeedVariance, rotationSpeedVariance);

        _freqY = verticalFrequency    + Random.Range(-frequencyVariance, frequencyVariance);
        _freqX = horizontalFrequencyX + Random.Range(-frequencyVariance, frequencyVariance);
        _freqZ = horizontalFrequencyZ + Random.Range(-frequencyVariance, frequencyVariance);

        _ampY = verticalAmplitude    + Random.Range(-amplitudeVariance, amplitudeVariance);
        _ampX = horizontalAmplitudeX + Random.Range(-amplitudeVariance * 0.5f, amplitudeVariance * 0.5f);
        _ampZ = horizontalAmplitudeZ + Random.Range(-amplitudeVariance * 0.5f, amplitudeVariance * 0.5f);

        // random start phase — main source of desync
        _phaseY = Random.Range(0f, Mathf.PI * 2f);
        _phaseX = Random.Range(0f, Mathf.PI * 2f);
        _phaseZ = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        _time += Time.deltaTime;

        if (_rotationEnabled)
            transform.Rotate(0f, 0f, _rotSpeed * Time.deltaTime, Space.Self);

        if (_levitationEnabled)
        {
            float t = _time * Mathf.PI * 2f;
            float y = Mathf.Sin(t * _freqY + _phaseY) * _ampY;
            float x = Mathf.Sin(t * _freqX + _phaseX) * _ampX;
            float z = Mathf.Cos(t * _freqZ + _phaseZ) * _ampZ;
            transform.localPosition = _originLocalPosition + new Vector3(x, y, z);
        }
    }
}
