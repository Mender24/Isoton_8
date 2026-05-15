using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [SerializeField] private Transform targetWaypoint;
    [SerializeField] private float maxSpeed = 5f;

    [Header("Acceleration & Deceleration")]
    [SerializeField] private float accelerationTime = 2f;
    [SerializeField] private float decelerationTime = 1.5f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Stopping")]
    [SerializeField] private float startDecelerationDistance = 3f;
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private bool smoothStop = true;

    [Header("General")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Movement Sound")]
    [SerializeField] private AudioSource hummingAudioSource;

    [Header("Debug")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float accelerationProgress = 0f;
    [SerializeField] private float decelerationProgress = 0f;
    [SerializeField] private bool hasReachedTarget = false;
    [SerializeField] private bool isDecelerating = false;

    private float startDelayTimer = 0f;
    private bool isWaitingForStart = false;
    private Vector3 lastTargetPosition;
    private float distanceToTarget;

    void Update()
    {
        if (!isActive || targetWaypoint == null)
            return;

        if (isWaitingForStart)
        {
            HandleStartDelay();
            return;
        }

        if (hasReachedTarget)
            return;

        distanceToTarget = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distanceToTarget <= stoppingDistance)
        {
            ReachedTarget();
            return;
        }

        bool shouldDecelerate = distanceToTarget <= startDecelerationDistance;

        if (shouldDecelerate)
        {
            UpdateDeceleration();
        }
        else
        {
            UpdateAcceleration();
        }

        MoveToWaypoint();
        UpdateHummingAudio();
    }

    private void HandleStartDelay()
    {
        startDelayTimer -= Time.deltaTime;

        if (startDelayTimer <= 0)
        {
            isWaitingForStart = false;
            accelerationProgress = 0f;
            decelerationProgress = 0f;
            currentSpeed = 0f;
            isDecelerating = false;
            hasReachedTarget = false;
            Debug.Log("�������� ���������. �������� ��������!");
        }
    }

    private void UpdateAcceleration()
    {
        isDecelerating = false;

        if (accelerationTime > 0 && accelerationProgress < 1f)
        {
            accelerationProgress += Time.deltaTime / accelerationTime;
            accelerationProgress = Mathf.Clamp01(accelerationProgress);
        }

        decelerationProgress = 0f;

        float accelerationFactor = accelerationCurve.Evaluate(accelerationProgress);
        currentSpeed = maxSpeed * accelerationFactor;
    }

    private void UpdateDeceleration()
    {
        if (!smoothStop)
        {
            float t = 1f - Mathf.Clamp01(distanceToTarget / startDecelerationDistance);
            currentSpeed = Mathf.Lerp(maxSpeed, 0f, t);
            return;
        }

        isDecelerating = true;

        float targetDecelerationProgress = 1f - Mathf.Clamp01(distanceToTarget / startDecelerationDistance);

        if (decelerationTime > 0)
        {
            float decelerationSpeed = 1f / decelerationTime;
            decelerationProgress = Mathf.MoveTowards(decelerationProgress, targetDecelerationProgress,
                                                     decelerationSpeed * Time.deltaTime);
        }
        else
        {
            decelerationProgress = targetDecelerationProgress;
        }

        accelerationProgress = 0f;

        float decelerationFactor = decelerationCurve.Evaluate(decelerationProgress);
        currentSpeed = maxSpeed * decelerationFactor;
    }

    private void MoveToWaypoint()
    {
        if (targetWaypoint.position != lastTargetPosition)
        {
            if (!isDecelerating)
            {
                accelerationProgress = Mathf.Max(0, accelerationProgress - 0.1f);
            }
        }
        lastTargetPosition = targetWaypoint.position;

        Vector3 direction = (targetWaypoint.position - transform.position).normalized;

        float step = currentSpeed * Time.deltaTime;

        if (step > distanceToTarget)
        {
            transform.position = targetWaypoint.position;
        }
        else
        {
            transform.position += direction * step;
        }

        Debug.DrawLine(transform.position, targetWaypoint.position,
                       isDecelerating ? Color.red : Color.green);
    }

    private void UpdateHummingAudio()
    {
        if (hummingAudioSource == null) return;

        float normalizedSpeed = maxSpeed > 0f ? Mathf.Clamp01(currentSpeed / maxSpeed) : 0f;
        hummingAudioSource.volume = normalizedSpeed;

        if (normalizedSpeed > 0.001f && !hummingAudioSource.isPlaying)
            hummingAudioSource.Play();
        else if (normalizedSpeed <= 0.001f && hummingAudioSource.isPlaying)
            hummingAudioSource.Stop();
    }

    private void StopHummingAudio()
    {
        if (hummingAudioSource == null) return;
        hummingAudioSource.volume = 0f;
        hummingAudioSource.Stop();
    }

    private void ReachedTarget()
    {
        hasReachedTarget = true;
        currentSpeed = 0f;
        accelerationProgress = 0f;
        decelerationProgress = 0f;
        isDecelerating = false;
        transform.position = targetWaypoint.position;
        StopHummingAudio();

        Debug.Log($"��������� ������� waypoint: {targetWaypoint.name}");
        OnTargetReached();
    }

    private void OnTargetReached()
    {

    }

    public void SetTargetWaypoint(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("������� waypoint �� ����� ���� null!");
            return;
        }

        targetWaypoint = newTarget;
        ResetMovement();
        Debug.Log($"���������� ����� ������� waypoint: {newTarget.name}");
    }

    public void StartMoving()
    {
        if (targetWaypoint == null)
        {
            Debug.LogWarning("�� ���������� ������� waypoint!");
            return;
        }

        isActive = true;
        hasReachedTarget = false;

        if (startDelay > 0)
        {
            isWaitingForStart = true;
            startDelayTimer = startDelay;
            Debug.Log($"�������� � {targetWaypoint.name} �������� ����� {startDelay} ������...");
        }
        else
        {
            isWaitingForStart = false;
            accelerationProgress = 0f;
            decelerationProgress = 0f;
            currentSpeed = 0f;
            isDecelerating = false;
            Debug.Log($"�������� � {targetWaypoint.name} ������!");
        }
    }
    public void MoveToTarget(Transform target)
    {
        SetTargetWaypoint(target);
        StartMoving();
    }

    public void MoveToTarget(Transform target, float speed)
    {
        maxSpeed = speed;
        SetTargetWaypoint(target);
        StartMoving();
    }

    public void MoveToTarget(Transform target, float speed, float delay, float accelTime, float decelTime = -1f)
    {
        maxSpeed = speed;
        startDelay = delay;
        accelerationTime = accelTime;

        if (decelTime > 0)
        {
            decelerationTime = decelTime;
        }

        SetTargetWaypoint(target);
        StartMoving();
    }

    public void StopMoving()
    {
        isActive = false;
        isWaitingForStart = false;
        currentSpeed = 0f;
        accelerationProgress = 0f;
        decelerationProgress = 0f;
        isDecelerating = false;
        StopHummingAudio();
        Debug.Log("�������� �����������");
    }

    public void ResetMovement()
    {
        StopMoving();
        hasReachedTarget = false;
    }

    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }

    public float GetDistanceToTarget()
    {
        if (targetWaypoint == null)
            return -1f;

        return Vector3.Distance(transform.position, targetWaypoint.position);
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public Transform GetTargetWaypoint()
    {
        return targetWaypoint;
    }

    public void SetMaxSpeed(float speed)
    {
        maxSpeed = Mathf.Max(0, speed);
    }

    public void SetAccelerationTime(float time)
    {
        accelerationTime = Mathf.Max(0, time);
    }

    public void SetDecelerationTime(float time)
    {
        decelerationTime = Mathf.Max(0, time);
    }

    public void SetStartDecelerationDistance(float distance)
    {
        startDecelerationDistance = Mathf.Max(0, distance);
    }

    public void SetStartDelay(float delay)
    {
        startDelay = Mathf.Max(0, delay);
    }

    public bool IsDecelerating()
    {
        return isDecelerating;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetWaypoint != null)
        {
            Gizmos.color = isDecelerating ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, targetWaypoint.position);

            Gizmos.color = hasReachedTarget ? Color.green : Color.red;
            Gizmos.DrawSphere(targetWaypoint.position, 0.3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetWaypoint.position, stoppingDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetWaypoint.position, startDecelerationDistance);

            Gizmos.color = Color.white;
            Vector3 speedDirection = (targetWaypoint.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, speedDirection * (currentSpeed / maxSpeed) * 2f);
        }
    }
}