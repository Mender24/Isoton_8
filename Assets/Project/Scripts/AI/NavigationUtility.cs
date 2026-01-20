using UnityEngine;
using UnityEngine.AI;

internal static class NavigationUtility
{
    public static bool UpdateAnimatorSpeed(Animator animator, string speedParameterName, NavMeshAgent navMeshAgent, float currentSpeed, float minSpeedThreshold = 0.1f, float explicitSpeed = -1f)
    {
        if (animator == null || string.IsNullOrEmpty(speedParameterName))
        {
            return false;
        }

        float num = 0f;
        num = ((explicitSpeed >= 0f) ? explicitSpeed : ((!(navMeshAgent != null)) ? currentSpeed : navMeshAgent.velocity.magnitude));
        if (num <= minSpeedThreshold)
        {
            num = 0f;
        }

        animator.SetFloat(speedParameterName, num);
        return true;
    }

    public static float SimpleMoveTowardsLocation(Transform agentTransform, Vector3 targetLocation, float speed, float distance, float slowDownDistance = 0f, float minSpeedRatio = 0.1f)
    {
        if (agentTransform == null)
        {
            return 0f;
        }

        Vector3 position = agentTransform.position;
        float num = speed;
        if (slowDownDistance > 0f && distance < slowDownDistance)
        {
            float num2 = distance / slowDownDistance;
            num = Mathf.Max(speed * minSpeedRatio, speed * num2);
        }

        Vector3 vector = targetLocation - position;
        vector.y = 0f;
        if (vector.sqrMagnitude > 0.0001f)
        {
            vector.Normalize();
            position += vector * (num * Time.deltaTime);
            agentTransform.position = position;
            agentTransform.forward = vector;
        }

        return num;
    }
}