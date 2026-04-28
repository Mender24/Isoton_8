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

    private TypeRotation _currentTypeRotation = TypeRotation.X;
    private Vector3 _direction = Vector3.right;

    private void Update()
    {
        if(_currentTypeRotation != _rotationType)
        {
            _currentTypeRotation = _rotationType;
            _direction = _currentTypeRotation == TypeRotation.X ? Vector3.right : (_currentTypeRotation == TypeRotation.Y) ? Vector3.up : Vector3.forward;
        }

        transform.Rotate(_direction, _speedRotation * Time.deltaTime);
    }
}