using UnityEngine;

/// <summary>
/// Скрипт на лампочке. Управляет Light-компонентом и (опционально) эмиссией меша лампочки.
/// Методы TurnOn / TurnOff / Toggle подключаются к OnInteract-событию Interactable на кнопке.
/// </summary>
public class LightBulb : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light _light;

    [Header("Bulb Mesh Emission (optional)")]
    [Tooltip("Рендерер меша лампочки — чтобы она визуально гасла")]
    [SerializeField] private Renderer _bulbRenderer;
    [SerializeField] private int _emissionMaterialIndex = 0;
    [SerializeField] private Color _emissionColorOn = Color.white;

    [Header("State")]
    [SerializeField] private bool _isOn = true;

    private Material _bulbMaterial;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public bool IsOn => _isOn;

    private void Awake()
    {
        if (_light == null)
            _light = GetComponentInChildren<Light>();

        if (_bulbRenderer != null)
            _bulbMaterial = _bulbRenderer.materials[_emissionMaterialIndex];

        Apply();
    }

    public void TurnOn()
    {
        _isOn = true;
        Apply();
    }

    public void TurnOff()
    {
        _isOn = false;
        Apply();
    }

    public void Toggle()
    {
        _isOn = !_isOn;
        Apply();
    }

    private void Apply()
    {
        if (_light != null)
            _light.enabled = _isOn;

        if (_bulbMaterial != null)
        {
            if (_isOn)
            {
                _bulbMaterial.EnableKeyword("_EMISSION");
                _bulbMaterial.SetColor(EmissionColor, _emissionColorOn);
            }
            else
            {
                _bulbMaterial.DisableKeyword("_EMISSION");
                _bulbMaterial.SetColor(EmissionColor, Color.black);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        Apply();
    }
#endif
}
