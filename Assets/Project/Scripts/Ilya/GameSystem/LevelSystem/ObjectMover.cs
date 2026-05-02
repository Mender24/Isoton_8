using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _len = 7535.646f;
    [Space]
    [SerializeField] private GameObject _rootObject;
    [SerializeField] private GameObject _additionalObject;
    [Space]
    [SerializeField] private float _speedChangeValueUp = 1f;
    [SerializeField] private float _speedChangeValueDown = 1f;

    private float _currentSpeed = 0;
    [SerializeField] private bool _isStop = false;

    private float _border;

    private DrillController _drillController;

    private void OnDestroy()
    {
        _drillController.Started -= OnStarted;
        _drillController.Stopped -= OnStopped;
    }

    private void Start()
    {
        _border = _rootObject.transform.localPosition.y + _len;

        _drillController = transform.parent.transform.parent.GetComponent<DrillController>();
        _drillController.Started += OnStarted;
        _drillController.Stopped += OnStopped;
    }

    private void Update()
    {
        ChangeSpeed();

        _rootObject.transform.Translate(Vector3.up * _currentSpeed * Time.deltaTime);
        _additionalObject.transform.Translate(Vector3.up * _currentSpeed * Time.deltaTime);

        if (_rootObject.transform.localPosition.y >= _border)
            _rootObject.transform.localPosition = new Vector3(_rootObject.transform.localPosition.x, -1719.3f, _rootObject.transform.localPosition.z);

        if (_additionalObject.transform.localPosition.y >= _border)
            _additionalObject.transform.localPosition = new Vector3(_additionalObject.transform.localPosition.x, -1719.3f, _additionalObject.transform.localPosition.z);
    }

    private void ChangeSpeed()
    {
        float targetSpeed;

        if (!_isStop)
            targetSpeed = _speed;
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
