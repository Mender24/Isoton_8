using System;
using UnityEngine;

public class TurretRotation : MonoBehaviour
{
    [SerializeField] TurretEnemy turretEnemy;
[Header("”глы поворота (по оси Y)")]
    public float fromAngle = -45f;
    public float toAngle = 45f;
    public float speed = 45f;
    [Range(0f, 3f)]
    public float pauseDuration = 0.5f; // задержка в крайней позиции

    private float _direction = 1f;
    private float _pauseTimer = 0f;
    private bool _isPaused = false;

    void Update()
    {
        if (turretEnemy._haveTarget == false)
            Rotate();
    }

    void Rotate()
    {
        if (_isPaused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
                _isPaused = false;
            return;
        }

        float current = transform.localEulerAngles.y;
        if (current > 180f) current -= 360f;

        float target = _direction > 0 ? toAngle : fromAngle;

        current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, current, transform.localEulerAngles.z);

        if (current >= toAngle)
        {
            _direction = -1f;
            _isPaused = true;
            _pauseTimer = pauseDuration;
        }
        else if (current <= fromAngle)
        {
            _direction = 1f;
            _isPaused = true;
            _pauseTimer = pauseDuration;
        }
    }
}