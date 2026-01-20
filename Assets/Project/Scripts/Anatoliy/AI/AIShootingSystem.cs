using Akila.FPSFramework;
using TMPro;
using UnityEngine;

public class AIShootingSystem : MonoBehaviour
{
    [SerializeField] private Bullet _prefabBullet;
    [SerializeField] private GameObject _spawnPointBullet;
    [Header("OffSet")]
    [SerializeField] private float _xValue; // const offset
    [SerializeField] private float _yValue;
    [Header("SizeColliderTarget")]
    [SerializeField] private float _height; // random offset
    [SerializeField] private float _width;

    private float _percentage;
    private float _bulletSpeed;

    public void Init(float percentage, float speed)
    {
        _percentage = percentage;
        _bulletSpeed = speed;
    }

    public void Fire(Damageable target)
    {
        if(target == null) 
            return;

        //Visualisation

        Vector3 targetPosition = target.transform.position;
        float yValue = Random.Range(-_height, _height);
        float xValue = Random.Range(-_width, _width);

        targetPosition.y += yValue + _yValue;
        targetPosition.x += xValue + _xValue;

        Bullet newBullet = Instantiate(_prefabBullet, transform);
        newBullet.transform.position = _spawnPointBullet.transform.position;
        newBullet.Init(2, (targetPosition - transform.position).normalized, _bulletSpeed);

        //Shoot

        targetPosition.y -= yValue;
        targetPosition.x -= xValue;

        float isShoot = Random.Range(0, 1f);

        if (isShoot <= _percentage)
        {
            RaycastHit hit;

            if (Physics.Raycast(_spawnPointBullet.transform.position, (targetPosition - _spawnPointBullet.transform.position).normalized, out hit, 100, LayerMask.GetMask("Player")))
            {
                Debug.DrawRay(_spawnPointBullet.transform.position, (targetPosition - _spawnPointBullet.transform.position).normalized * 100, Color.blue, 0.5f);
                Debug.Log("Target: " + hit.collider.gameObject.name + " Hit: " + (isShoot));
            }
        }
    }
}
