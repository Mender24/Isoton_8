using UnityEngine;

[RequireComponent(typeof(RangedCombatModule))]
public class TurretRecoil : MonoBehaviour
{
    [Header("Primary Barrel")]
    [SerializeField] private Transform _barrelPrimary;
    [SerializeField] private Vector3 _recoilOffsetPrimary = new Vector3(0f, 0f, -0.1f); // назад по Z
    [SerializeField] private float _recoilSpeedPrimary = 8f;

    [Header("Secondary Barrel")]
    [SerializeField] private Transform _barrelSecondary;
    [SerializeField] private Vector3 _recoilOffsetSecondary = new Vector3(0f, 0f, -0.1f);
    [SerializeField] private float _recoilSpeedSecondary = 8f;

    private RangedCombatModule _rangedCombat;
    private bool _useDoubleBarrelTurret;


    // Смещение от нейтрального положения
    private Vector3 _barrelPrimaryOffset;
    private Vector3 _barrelSecondaryOffset;

    private Vector3 _barrelPrimaryNeutralPos;
    private Vector3 _barrelSecondaryNeutralPos;

    private void Awake()
    {
        _rangedCombat = GetComponent<RangedCombatModule>();
        _useDoubleBarrelTurret = _rangedCombat.GetUseDoubleBarrelTurret();

        // Сохраняем нейтральные позиции стволов
        if (_barrelPrimary != null)
            _barrelPrimaryNeutralPos = _barrelPrimary.localPosition;

        if (_barrelSecondary != null)
            _barrelSecondaryNeutralPos = _barrelSecondary.localPosition;
    }

    private void OnEnable() => _rangedCombat.OnFire += OnFire;
    private void OnDisable() => _rangedCombat.OnFire -= OnFire;

    private void OnFire()
    {
        // Узнаём, какой сейчас ствол стреляет (из RangedCombatModule)
        // Поэтому нужно будет добавить геттер в RangedCombatModule
        if (_rangedCombat == null) return;

        bool isPrimaryFiring = _rangedCombat._useDoubleBarrelTurret
            ? _rangedCombat._usePrimaryBarrel
            : true;

        if (_barrelPrimary != null)
        {
            // Откат только того ствола, который стреляет
            if (isPrimaryFiring)
                _barrelPrimaryOffset += _recoilOffsetPrimary;
            else
                _barrelSecondaryOffset += _recoilOffsetSecondary;
        }
    }

    private void Update()
    {
        if (_barrelPrimary != null)
        {
            // Плавно возвращаемся к нейтральному отступу (0)
            _barrelPrimaryOffset = Vector3.Lerp(
                _barrelPrimaryOffset, Vector3.zero,
                _recoilSpeedPrimary * Time.deltaTime);

            // Финальный положение = нейтральное + откат
            _barrelPrimary.localPosition = _barrelPrimaryNeutralPos + _barrelPrimaryOffset;
        }

        if (_barrelSecondary != null && _useDoubleBarrelTurret)
        {
            _barrelSecondaryOffset = Vector3.Lerp(
                _barrelSecondaryOffset, Vector3.zero,
                _recoilSpeedSecondary * Time.deltaTime);

            _barrelSecondary.localPosition = _barrelSecondaryNeutralPos + _barrelSecondaryOffset;
        }
    }
}