using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    private enum TypeRotation
    {
        X,
        Y,
        Z,
    }

    [SerializeField] private float _speedRotation = 4f;
    [SerializeField] private TypeRotation _rotationType = TypeRotation.X;
    [Space]
    [SerializeField] private float _speedChangeValueUp = 1f;
    [SerializeField] private float _speedChangeValueDown = 1f;

    private TypeRotation _currentTypeRotation = TypeRotation.X;
    private Vector3 _direction = Vector3.right;

    private float _currentSpeed = 0;
    [SerializeField] private bool _isStop = false;

    private DrillController _drillController;

    private void OnDestroy()
    {
        _drillController.Started -= OnStarted;
        _drillController.Stopped -= OnStopped;
    }

    private void Start()
    {
        _drillController = transform.parent.parent.parent.GetComponent<DrillController>();
        _drillController.Started += OnStarted;
        _drillController.Stopped += OnStopped;
    }

    private void Update()
    {
        ChangeSpeed();

        if (_currentTypeRotation != _rotationType)
        {
            _currentTypeRotation = _rotationType;
            _direction = _currentTypeRotation == TypeRotation.X ? Vector3.right : (_currentTypeRotation == TypeRotation.Y) ? Vector3.up : Vector3.forward;
        }

        transform.Rotate(_direction, _currentSpeed * Time.deltaTime);
    }

    private void ChangeSpeed()
    {
        float targetSpeed;

        if (!_isStop)
            targetSpeed = _speedRotation;
        else
            targetSpeed = 0;

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, (_isStop ? _speedChangeValueDown : _speedChangeValueUp) * Time.deltaTime);
    }

    private void OnStarted()
    {
        _isStop = false;
    }

    private void OnStopped()
    {
        _isStop = true;
    }
}