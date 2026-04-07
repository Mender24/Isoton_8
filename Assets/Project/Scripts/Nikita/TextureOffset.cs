using UnityEngine;

public class TextureOffset : MonoBehaviour
{
    [Header("Main Settings")]
    [SerializeField] private Material _targetMaterial; // ���� null, ���������� �������� �� Renderer
    [SerializeField] private int _materialIndex = 0; // ������ ��������� (��� �������� � ����������� �����������)

    [Header("Movement Settings")]
    [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.1f, 0.1f); // �������� �������� �� X � Y
    [SerializeField] private bool _useSmoothPingPong = false; // �������� ������� �������� �����-������
    [SerializeField] private Vector2 _pingPongRange = new Vector2(0f, 1f); // �������� �������� (���, ����)
    [SerializeField] private Vector2 _pingPongSpeed = new Vector2(0.5f, 0.5f); // �������� ���������

    [Header("Advanced Settings")]
    [SerializeField] private bool _applyToAllMaterials = false; // ��������� �� ���� ���������� �������
    [SerializeField] private bool _useGlobalTime = true; // ������������ Time.time ������ ������������ �������
    [SerializeField] private bool _resetOnDisable = false; // ���������� offset ��� ����������

    private Renderer _renderer;
    private Material _material;
    private Vector2 _currentOffset;
    private float _timeX;
    private float _timeY;

    // �������� ��� ������� �����
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

    /// <summary>
    /// Повторная инициализация — вызывать после смены материала на рендерере
    /// </summary>
    public void Reinitialize()
    {
        Initialize();
    }

    private void Initialize()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError($"TextureOffset: �� ������� {gameObject.name} ��� ���������� Renderer!");
            enabled = false;
            return;
        }

        // ���� �������� �� ��������, ����� �� ���������
        if (_targetMaterial == null && _renderer != null)
        {
            if (_materialIndex < _renderer.materials.Length)
            {
                _targetMaterial = _renderer.materials[_materialIndex];
            }
            else
            {
                Debug.LogError($"TextureOffset: ������ ��������� {_materialIndex} ������� �� ������� ������� ���������� �� {gameObject.name}");
                enabled = false;
                return;
            }
        }

        _material = _targetMaterial;

        // �������� ������� offset
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
    /// �������� �������� ��������
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

        // �����������: ����������� �������� (��� �������������� ������������ float)
        if (Mathf.Abs(_currentOffset.x) > 1000f) _currentOffset.x -= Mathf.Sign(_currentOffset.x) * 1000f;
        if (Mathf.Abs(_currentOffset.y) > 1000f) _currentOffset.y -= Mathf.Sign(_currentOffset.y) * 1000f;
    }

    /// <summary>
    /// ������� ��������� �������� (�����-������, �����-����)
    /// </summary>
    private void UpdatePingPongOffset()
    {
        // ��������� ����� ��� ������ ���
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

        // ������������ offset � ������� PingPong
        float pingPongX = Mathf.PingPong(_timeX, _pingPongRange.y - _pingPongRange.x) + _pingPongRange.x;
        float pingPongY = Mathf.PingPong(_timeY, _pingPongRange.y - _pingPongRange.x) + _pingPongRange.x;

        // ���� ���� ������� �������� ��������, ��������� � � ���������
        _currentOffset.x = pingPongX + (_useGlobalTime ? Time.time * _scrollSpeed.x : _currentOffset.x + _scrollSpeed.x * Time.deltaTime);
        _currentOffset.y = pingPongY + (_useGlobalTime ? Time.time * _scrollSpeed.y : _currentOffset.y + _scrollSpeed.y * Time.deltaTime);
    }

    /// <summary>
    /// ��������� ������� offset � ���������(��)
    /// </summary>
    private void ApplyOffset()
    {
        if (_applyToAllMaterials && _renderer != null)
        {
            // ��������� �� ���� ����������
            foreach (Material mat in _renderer.materials)
            {
                if (mat != null)
                    mat.mainTextureOffset = _currentOffset;
            }
        }
        else
        {
            // ��������� ������ � �������� ���������
            if (_material != null)
                _material.mainTextureOffset = _currentOffset;
        }
    }

    /// <summary>
    /// ���������� offset �������
    /// </summary>
    public void SetOffset(Vector2 offset)
    {
        _currentOffset = offset;
        ApplyOffset();
    }

    /// <summary>
    /// ���������� offset �� X
    /// </summary>
    public void SetOffsetX(float x)
    {
        _currentOffset.x = x;
        ApplyOffset();
    }

    /// <summary>
    /// ���������� offset �� Y
    /// </summary>
    public void SetOffsetY(float y)
    {
        _currentOffset.y = y;
        ApplyOffset();
    }

    /// <summary>
    /// �������� offset � ����
    /// </summary>
    public void ResetOffset()
    {
        _currentOffset = Vector2.zero;
        ApplyOffset();
    }

    /// <summary>
    /// �������� ���������� �������� (�������� ��������)
    /// </summary>
    public void PauseMovement()
    {
        _scrollSpeed = Vector2.zero;
        _pingPongSpeed = Vector2.zero;
    }

    /// <summary>
    /// ����������� �������� � ������ �����������
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
        // �������������: ���������� �������� � �������� ��������� ��� �����������
        if (_resetOnDisable && _material != null)
        {
            ResetOffset();
        }
    }
}