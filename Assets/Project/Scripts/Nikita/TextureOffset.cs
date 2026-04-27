using UnityEngine;

public class TextureOffset : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] private Material _targetMaterial; // Если null, использует материал из Renderer
    [SerializeField] private int _materialIndex = 0; // Индекс материала (для объектов с несколькими материалами)

    [Header("Movement Settings")]
    [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.1f, 0.1f); // Скорость движения по X и Y
    [SerializeField] private bool _useSmoothPingPong = false; // Включить плавное движение влево-вправо
    [SerializeField] private Vector2 _pingPongRange = new Vector2(0f, 1f); // Диапазон движения (мин, макс)
    [SerializeField] private Vector2 _pingPongSpeed = new Vector2(0.5f, 0.5f); // Скорость колебаний

    [Header("Advanced Settings")]
    [SerializeField] private bool _applyToAllMaterials = false; // Применять ко всем материалам объекта
    [SerializeField] private bool _useGlobalTime = true; // Использовать Time.time вместо накопленного времени
    [SerializeField] private bool _resetOnDisable = false; // Сбрасывать offset при отключении

    private Renderer _renderer;
    private Material _material;
    private Vector2 _currentOffset;
    private float _timeX;
    private float _timeY;

    // Свойства для доступа извне
    public Vector2 ScrollSpeed
    {
        get => _scrollSpeed;
        set => _scrollSpeed = value;
    }

    public Vector2 CurrentOffset => _currentOffset;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError($"TextureOffset: На объекте {gameObject.name} нет компонента Renderer!");
            enabled = false;
            return;
        }

        // Если материал не назначен, берем из рендерера
        if (_targetMaterial == null && _renderer != null)
        {
            if (_materialIndex < _renderer.materials.Length)
            {
                _targetMaterial = _renderer.materials[_materialIndex];
            }
            else
            {
                Debug.LogError($"TextureOffset: Индекс материала {_materialIndex} выходит за пределы массива материалов на {gameObject.name}");
                enabled = false;
                return;
            }
        }

        _material = _targetMaterial;

        // Получаем текущий offset
        if (_material != null)
        {
            _currentOffset = _material.mainTextureOffset;
        }
    }

    private void Update()
    {
        if (_material == null) return;

        if (_useSmoothPingPong)
        {
            UpdatePingPongOffset();
        }
        else
        {
            UpdateLinearOffset();
        }

        ApplyOffset();
    }

    /// <summary>
    /// Линейное движение текстуры
    /// </summary>
    private void UpdateLinearOffset()
    {
        if (_useGlobalTime)
        {
            _currentOffset.x = Time.time * _scrollSpeed.x;
            _currentOffset.y = Time.time * _scrollSpeed.y;
        }
        else
        {
            _currentOffset.x += _scrollSpeed.x * Time.deltaTime;
            _currentOffset.y += _scrollSpeed.y * Time.deltaTime;
        }

        // Опционально: зацикливаем значения (для предотвращения переполнения float)
        if (Mathf.Abs(_currentOffset.x) > 1000f) _currentOffset.x -= Mathf.Sign(_currentOffset.x) * 1000f;
        if (Mathf.Abs(_currentOffset.y) > 1000f) _currentOffset.y -= Mathf.Sign(_currentOffset.y) * 1000f;
    }

    /// <summary>
    /// Плавное колебание текстуры (влево-вправо, вверх-вниз)
    /// </summary>
    private void UpdatePingPongOffset()
    {
        // Обновляем время для каждой оси
        if (_useGlobalTime)
        {
            _timeX = Time.time * _pingPongSpeed.x;
            _timeY = Time.time * _pingPongSpeed.y;
        }
        else
        {
            _timeX += _pingPongSpeed.x * Time.deltaTime;
            _timeY += _pingPongSpeed.y * Time.deltaTime;
        }

        // Рассчитываем offset с помощью PingPong
        float pingPongX = Mathf.PingPong(_timeX, _pingPongRange.y - _pingPongRange.x) + _pingPongRange.x;
        float pingPongY = Mathf.PingPong(_timeY, _pingPongRange.y - _pingPongRange.x) + _pingPongRange.x;

        // Если есть базовая скорость движения, добавляем её к колебанию
        _currentOffset.x = pingPongX + (_useGlobalTime ? Time.time * _scrollSpeed.x : _currentOffset.x + _scrollSpeed.x * Time.deltaTime);
        _currentOffset.y = pingPongY + (_useGlobalTime ? Time.time * _scrollSpeed.y : _currentOffset.y + _scrollSpeed.y * Time.deltaTime);
    }

    /// <summary>
    /// Применяет текущий offset к материалу(ам)
    /// </summary>
    private void ApplyOffset()
    {
        if (_applyToAllMaterials && _renderer != null)
        {
            // Применяем ко всем материалам
            foreach (Material mat in _renderer.materials)
            {
                if (mat != null)
                    mat.mainTextureOffset = _currentOffset;
            }
        }
        else
        {
            // Применяем только к целевому материалу
            if (_material != null)
                _material.mainTextureOffset = _currentOffset;
        }
    }

    /// <summary>
    /// Установить offset вручную
    /// </summary>
    public void SetOffset(Vector2 offset)
    {
        _currentOffset = offset;
        ApplyOffset();
    }

    /// <summary>
    /// Установить offset по X
    /// </summary>
    public void SetOffsetX(float x)
    {
        _currentOffset.x = x;
        ApplyOffset();
    }

    /// <summary>
    /// Установить offset по Y
    /// </summary>
    public void SetOffsetY(float y)
    {
        _currentOffset.y = y;
        ApplyOffset();
    }

    /// <summary>
    /// Сбросить offset в ноль
    /// </summary>
    public void ResetOffset()
    {
        _currentOffset = Vector2.zero;
        ApplyOffset();
    }

    /// <summary>
    /// Временно остановить движение (обнуляет скорость)
    /// </summary>
    public void PauseMovement()
    {
        _scrollSpeed = Vector2.zero;
        _pingPongSpeed = Vector2.zero;
    }

    /// <summary>
    /// Возобновить движение с новыми параметрами
    /// </summary>
    public void ResumeMovement(Vector2 scrollSpeed, Vector2? pingPongSpeed = null)
    {
        _scrollSpeed = scrollSpeed;
        if (pingPongSpeed.HasValue)
            _pingPongSpeed = pingPongSpeed.Value;
    }

    private void OnDisable()
    {
        if (_resetOnDisable && _material != null)
        {
            ResetOffset();
        }
    }

    private void OnDestroy()
    {
        // Необязательно: возвращаем материал в исходное состояние при уничтожении
        if (_resetOnDisable && _material != null)
        {
            ResetOffset();
        }
    }
}