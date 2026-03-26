using UnityEngine;

/// <summary>
/// Переключает провод между двумя состояниями:
///   Isoton  — светящийся материал + движение текстуры (TextureOffset включён)
///   Rubber  — резиновый материал + движение отключено
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WireStateSwitcher : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _isotonMaterial;
    [SerializeField] private Material _rubberMaterial;
    [SerializeField] private int _materialIndex = 0;

    [Header("References")]
    [SerializeField] private TextureOffset _textureOffset;

    [Header("Editor Preview (Debug)")]
    [SerializeField] private bool _isotonActive = true; // Only for debug

    private Renderer _renderer;
    private bool _isIsotonActive = true;

    /// <summary>Текущее состояние: true = Isoton, false = Rubber</summary>
    public bool IsIsotonActive => _isIsotonActive;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
            Debug.LogError($"WireStateSwitcher: на объекте {gameObject.name} нет Renderer!");

        if (_textureOffset == null)
            _textureOffset = GetComponent<TextureOffset>();
    }

    /// <summary>Активировать светящееся состояние (Isoton + движение текстуры)</summary>
    public void ActivateIsoton()
    {
        if (_isIsotonActive) return;
        SwapMaterial(_isotonMaterial);
        if (_textureOffset != null)
        {
            _textureOffset.enabled = true;
            _textureOffset.Reinitialize(); // перехватить новый экземпляр материала
        }
        _isIsotonActive = true;
    }

    /// <summary>Активировать нейтральное состояние (Rubber, движение выключено)</summary>
    public void ActivateRubber()
    {
        if (!_isIsotonActive) return;
        SwapMaterial(_rubberMaterial);
        if (_textureOffset != null)
            _textureOffset.enabled = false;
        _isIsotonActive = false;
    }

    /// <summary>Переключить состояние на противоположное</summary>
    public void Toggle()
    {
        if (_isIsotonActive)
            ActivateRubber();
        else
            ActivateIsoton();
    }

    /// <summary>
    /// Явно задать состояние: true = Isoton, false = Rubber.
    /// Удобно для вызова из UnityEvent или другого скрипта.
    /// </summary>
    public void SetState(bool useIsoton)
    {
        if (useIsoton)
            ActivateIsoton();
        else
            ActivateRubber();
    }

    // ──────────────────────────────────────────────
    // Внутренняя логика
    // ──────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        // Применить нужный материал сразу при изменении чекбокса в инспекторе
        SwapMaterial(_isotonActive ? _isotonMaterial : _rubberMaterial);
        _isIsotonActive = _isotonActive;
    }
#endif

    private void SwapMaterial(Material target)
    {
        if (_renderer == null || target == null) return;

        Material[] mats = _renderer.sharedMaterials;

        if (_materialIndex >= mats.Length)
        {
            Debug.LogError($"WireStateSwitcher: materialIndex {_materialIndex} выходит за пределы массива материалов на {gameObject.name}");
            return;
        }

        mats[_materialIndex] = target;
        _renderer.sharedMaterials = mats;
    }
}
