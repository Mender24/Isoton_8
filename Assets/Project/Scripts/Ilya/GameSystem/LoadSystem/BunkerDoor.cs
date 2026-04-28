using UnityEngine;
using System.Collections;

public class BunkerDoor : MonoBehaviour
{
    [SerializeField] private bool _isMoveOpen = false;
    [SerializeField] private float _lenMoveDoor = 4f;
    [SerializeField] private float _speedMoveDoor = 4f;
    [Space]
    [SerializeField] private bool _openNow = false;
    //public Transform pivot_left;
    public BunkerDoor _doubleDoor;
    public Transform pivot_hinge;
    public float _angle = -90;
    public float roughness = 2;

    [Header("Auto Close Settings")]
    [SerializeField] private float _autoCloseDelay = 3f; // Время до автоматического закрытия
    [SerializeField] private bool _autoCloseEnabled = false; // Включено ли авто-закрытие

    private Quaternion targetRotation;
    private Vector3 _targetPosition = Vector3.zero;
    private bool _isOpen = false;
    public bool isOpenInitially = false;

    private Coroutine _autoCloseCoroutine; // Ссылка на корутину для возможности отмены

    public bool IsOpen => _isOpen;

    private void Start()
    {
        _targetPosition = pivot_hinge.position;

        if(isOpenInitially)
        {
            OpenDoor();
        }
    }

    private void Update()
    {
        if (!_isMoveOpen && targetRotation != pivot_hinge.localRotation)
            pivot_hinge.localRotation = Quaternion.Lerp(pivot_hinge.localRotation, Quaternion.Inverse(targetRotation), Time.deltaTime * roughness);

        if (_isMoveOpen && pivot_hinge.position != _targetPosition)
            pivot_hinge.position = Vector3.MoveTowards(pivot_hinge.position, _targetPosition, _speedMoveDoor * Time.deltaTime);

        if (_openNow)
        {
            _openNow = false;
            OpenDoor();
        }
    }

    public void ToggleDoor()
    {
        targetRotation = targetRotation == Quaternion.Euler(0, _angle, 0) ? Quaternion.identity : Quaternion.Euler(0, _angle, 0);
    }

    public void OpenDoor()
    {
        if (_isOpen)
            return;

        _isOpen = true;

        if (_doubleDoor != null)
            _doubleDoor.OpenDoor();

        if (!_isMoveOpen)
            targetRotation = Quaternion.Euler(0, _angle, 0);

        if (_isMoveOpen)
            _targetPosition.y += _lenMoveDoor;

        // Запускаем авто-закрытие, если оно включено
        StartAutoClose();
    }

    public void CloseDoor()
    {
        if (!_isOpen)
            return;

        _isOpen = false;

        if (_doubleDoor != null)
            _doubleDoor.CloseDoor();

        if (!_isMoveOpen)
            targetRotation = Quaternion.identity;

        if (_isMoveOpen)
            _targetPosition.y -= _lenMoveDoor;

        // Останавливаем корутину авто-закрытия, если дверь закрыли вручную
        StopAutoClose();
    }

    /// <summary>
    /// Запускает корутину автоматического закрытия двери
    /// </summary>
    private void StartAutoClose()
    {
        // Если авто-закрытие отключено, ничего не делаем
        if (!_autoCloseEnabled)
            return;

        // Останавливаем предыдущую корутину, если она была
        StopAutoClose();

        // Запускаем новую корутину
        _autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
    }

    /// <summary>
    /// Останавливает корутину автоматического закрытия
    /// </summary>
    private void StopAutoClose()
    {
        if (_autoCloseCoroutine != null)
        {
            StopCoroutine(_autoCloseCoroutine);
            _autoCloseCoroutine = null;
        }
    }

    /// <summary>
    /// Корутина, которая ждет N секунд и закрывает дверь
    /// </summary>
    private IEnumerator AutoCloseCoroutine()
    {
        // Ждем указанное количество секунд
        yield return new WaitForSeconds(_autoCloseDelay);

        // Закрываем дверь
        CloseDoor();

        _autoCloseCoroutine = null;
    }

    /// <summary>
    /// Публичный метод для установки задержки авто-закрытия извне
    /// </summary>
    public void SetAutoCloseDelay(float delay)
    {
        _autoCloseDelay = delay;
    }

    /// <summary>
    /// Публичный метод для включения/отключения авто-закрытия извне
    /// </summary>
    public void SetAutoCloseEnabled(bool enabled)
    {
        _autoCloseEnabled = enabled;

        // Если авто-закрытие отключаем, останавливаем корутину
        if (!enabled)
        {
            StopAutoClose();
        }
        // Если включаем и дверь открыта - запускаем заново
        else if (_isOpen)
        {
            StartAutoClose();
        }
    }
}