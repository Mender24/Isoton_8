using UnityEngine;

/// <summary>
/// Меняет цвет лампочки и светового источника в зависимости от режима турели.
/// Жёлтый   — враг не активен.
/// Красный  — режим PlayerOnly или Any.
/// Зелёный  — режим MutantsOnly.
/// </summary>
[RequireComponent(typeof(TurretEnemy))]
public class TurretLightIndicator : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private Renderer _lightRenderer; // лампочка/индикатор
    [SerializeField] private bool _useEmission = true; // использовать ли emission

    [Header("Light Component")]
    [SerializeField] private Light _lightComponent; // Light-компонент на турели

    [Header("Colors")]
    [SerializeField] private Color _offColor = Color.yellow;
    [SerializeField] private Color _playerColor = Color.red;
    [SerializeField] private Color _mutantsColor = Color.green;

    private TurretEnemy _turretEnemy;
    private Material _material;

    private void Awake()
    {
        _turretEnemy = GetComponent<TurretEnemy>();
        if (_lightRenderer != null)
            _material = _lightRenderer.material;
    }

    private void OnEnable()
    {
        _turretEnemy.OnTargetModeChanged += UpdateLightColor;
    }

    private void OnDisable()
    {
        _turretEnemy.OnTargetModeChanged -= UpdateLightColor;
    }

    private void LateUpdate()
    {
        // На случай, если активность меняется вне TargetMode
        UpdateLightColor();
    }

    private void UpdateLightColor()
    {
        Color targetColor = _offColor;

        // Если турель НЕ активна или мертва, делаем жёлтый
        if (!_turretEnemy.State.IsActivated || _turretEnemy.State.IsDead)
        {
            targetColor = _offColor;
        }
        else
        {
            switch (_turretEnemy.TargetMode)
            {
                case TurretEnemy.TurretTargetMode.PlayerOnly:
                case TurretEnemy.TurretTargetMode.Any:
                    targetColor = _playerColor;
                    break;
                case TurretEnemy.TurretTargetMode.MutantsOnly:
                    targetColor = _mutantsColor;
                    break;
            }
        }

        // Ставим цвет лампочки/индикатора
        if (_lightRenderer != null)
        {
            _lightRenderer.material.color = targetColor;

            if (_useEmission && _material != null)
            {
                float emissionMul = 1.5f;
                _material.SetColor("_EmissionColor", targetColor * emissionMul);
            }
        }

        // Если есть Light-компонент — меняем его цвет
        if (_lightComponent != null)
        {
            _lightComponent.color = targetColor;
        }
    }
}