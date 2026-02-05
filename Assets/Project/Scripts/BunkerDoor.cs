using UnityEngine;

public class BunkerDoor : MonoBehaviour
{
    [SerializeField] private bool _isMoveOpen = false;
    [SerializeField] private float _lenMoveDoor = 4f;
    [SerializeField] private float _speedMoveDoor = 4f;
    [Space]
    //public Transform pivot_left;
    public BunkerDoor _doubleDoor;
    public Transform pivot_hinge;
    public float _angle = -90;
    public float roughness = 2;

    private Quaternion targetRotation;
    private Vector3 _targetPosition = Vector3.zero;
    public bool isOpenInitially = false;

    private void Start()
    {
        _targetPosition = pivot_hinge.position;
    }

    private void Update()
    {
        if (!_isMoveOpen && targetRotation != pivot_hinge.localRotation)
            pivot_hinge.localRotation = Quaternion.Lerp(pivot_hinge.localRotation, Quaternion.Inverse(targetRotation), Time.deltaTime * roughness);

        if (_isMoveOpen && pivot_hinge.position != _targetPosition)
            pivot_hinge.position = Vector3.MoveTowards(pivot_hinge.position, _targetPosition, _speedMoveDoor * Time.deltaTime);
    }

    public void ToggleDoor()
    {
        targetRotation = targetRotation == Quaternion.Euler(0, _angle, 0) ? Quaternion.identity : Quaternion.Euler(0, _angle, 0);
    }

    public void OpenDoor()
    {
        if (_doubleDoor != null)
            _doubleDoor.OpenDoor();

        if (!_isMoveOpen)
            targetRotation = Quaternion.Euler(0, _angle, 0);

        if (_isMoveOpen)
            _targetPosition.y += _lenMoveDoor;
    }

    public void CloseDoor()
    {
        if (_doubleDoor != null)
            _doubleDoor.CloseDoor();

        if (!_isMoveOpen)
            targetRotation = Quaternion.identity;

        if (_isMoveOpen)
            _targetPosition.y -= _lenMoveDoor;
    }
}
