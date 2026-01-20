using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private float _lifeTime = 2f;
    private float _speed = 5f;
    private Vector3 _direction = Vector3.zero;

    private Rigidbody _rb;

    private void Update()
    {
        if(_lifeTime <= 0)
        {
            _lifeTime = int.MaxValue;
            PoolManager.Instance.SetObject(this);

            return;
        }

        _lifeTime -= Time.deltaTime;
    }

    public void Init(float lifeTime, Vector3 direction, float speed)
    {
        _rb = GetComponent<Rigidbody>();

        _lifeTime = lifeTime;
        _direction = direction;
        _speed = speed;

        _rb.linearVelocity = _direction * _speed;
    }
}
