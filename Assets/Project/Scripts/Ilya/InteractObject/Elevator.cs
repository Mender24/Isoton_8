using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float _endSpeed = 6f;
    [SerializeField] private float _addSpeed = 0.1f;

    private float _speed = 0f;
    private Transform _targetPoint;

    public void StartMoveElevator()
    {
        _targetPoint = transform.GetChild(0);
        StartCoroutine(MoveElevator());
    }

    private IEnumerator MoveElevator()
    {
        while(_targetPoint != null && _targetPoint.transform.position != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPoint.position, _speed * Time.deltaTime);
            yield return null;

            _speed += _addSpeed;

            if (_speed > _endSpeed)
                _speed = _endSpeed;
        }
    }
}
