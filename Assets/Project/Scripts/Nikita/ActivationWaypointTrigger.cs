using UnityEngine;

public class ActivationWaypointTrigger : MonoBehaviour
{
    [Header("��������� ��������")]
    [SerializeField] private string targetTag = "Player"; // ��� �������, ������� ���������� �������
    [SerializeField] private bool activateOnEnter = true; // ������������ ��� �����
    [SerializeField] private bool deactivateOnExit = false; // �������������� ��� ������
    [SerializeField] private bool oneTimeUse = false; // ������� ����������� ������ ���� ���
    [SerializeField] private bool requireKeyPress = false; // ��������� ������� �������
    [SerializeField] private KeyCode activationKey = KeyCode.E; // ������� ���������

    [Header("������� WaypointFollower")]
    [SerializeField] private WaypointFollower targetWaypointFollower;

    [Header("���������� �������� �����")]
    [SerializeField] private bool showActivationPrompt = true;
    // [SerializeField] private string activationMessage = "������� E ��� ���������";
    [SerializeField] private GameObject activationHint; // UI ������� ��� 3D ������ ��� ���������

    private bool isPlayerInTrigger = false;
    private bool hasBeenUsed = false;

    void Start()
    {
        // �������� ������� ���������� WaypointFollower
        if (targetWaypointFollower == null)
        {
            // ������� ����� �� ��� �� �������
            targetWaypointFollower = GetComponent<WaypointFollower>();

            // ���� �� �����, ���� �� ������������ �������
            if (targetWaypointFollower == null && transform.parent != null)
            {
                targetWaypointFollower = transform.parent.GetComponent<WaypointFollower>();
            }

            // ���� ��� ��� �� �����, ���� �� ����
            if (targetWaypointFollower == null)
            {
                GameObject waypointObject = GameObject.FindGameObjectWithTag("WaypointFollower");
                if (waypointObject != null)
                {
                    targetWaypointFollower = waypointObject.GetComponent<WaypointFollower>();
                }
            }

            if (targetWaypointFollower == null)
            {
                Debug.LogWarning($"WaypointFollower �� ������ ��� {gameObject.name}!");
            }
        }

        // ��������� ���������� �������� �����
        if (activationHint != null)
        {
            activationHint.SetActive(false);
        }
    }

    void Update()
    {
        // ��������� ��������� �� �������
        if (requireKeyPress && isPlayerInTrigger && !hasBeenUsed)
        {
            if (Input.GetKeyDown(activationKey))
            {
                ActivateWaypointFollower();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ��������� ��� �������, ��������� � �������
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = true;

            // ���������� ��������� ���� �����
            if (showActivationPrompt && activationHint != null)
            {
                activationHint.SetActive(true);
            }

            // ���� �� ��������� ������� ������� - ���������� �����
            if (activateOnEnter && !requireKeyPress && !hasBeenUsed)
            {
                ActivateWaypointFollower();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // ��������� ��� �������, ��������� �� ��������
        if (other.CompareTag(targetTag))
        {
            isPlayerInTrigger = false;

            // �������� ���������
            if (activationHint != null)
            {
                activationHint.SetActive(false);
            }

            // ������������ ���� �����
            if (deactivateOnExit && targetWaypointFollower != null)
            {
                targetWaypointFollower.StopMoving();
            }
        }
    }

    void ActivateWaypointFollower()
    {
        if (targetWaypointFollower == null)
        {
            Debug.LogError($"�� ���� ������������ WaypointFollower - ������ �� ��������!");
            return;
        }

        if (oneTimeUse && hasBeenUsed)
        {
            Debug.Log($"������� {gameObject.name} ��� �����������!");
            return;
        }

        // ���������� WaypointFollower
        targetWaypointFollower.StartMoving();
        Debug.Log($"WaypointFollower ����������� ��������� {gameObject.name}");

        // �������� ��� �������������� ���� �����������
        if (oneTimeUse)
        {
            hasBeenUsed = true;

            // ��������� ���������� ���������
            if (activationHint != null)
            {
                activationHint.SetActive(false);
            }

            // ��������� ��� ������� ���� �����
            if (GetComponent<Collider>() != null)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        // ������������ � ���������
        if (targetWaypointFollower != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetWaypointFollower.transform.position);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    // === ��������� ������ ��� ������� ���������� ===

    /// <summary>
    /// ������ ��������� WaypointFollower (����� �������� �� ������ ��������)
    /// </summary>
    public void ManualActivate()
    {
        ActivateWaypointFollower();
    }

    /// <summary>
    /// ������ ����������� WaypointFollower
    /// </summary>
    public void ManualDeactivate()
    {
        if (targetWaypointFollower != null)
        {
            targetWaypointFollower.StopMoving();
        }
    }

    /// <summary>
    /// ����� ��������� �������� (������� ��� ������������ ���������)
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenUsed = false;
        isPlayerInTrigger = false;

        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = true;
        }
    }

    /// <summary>
    /// ���������� ����� WaypointFollower
    /// </summary>
    public void SetTargetWaypointFollower(WaypointFollower newTarget)
    {
        targetWaypointFollower = newTarget;
    }

    /// <summary>
    /// �������� ������� WaypointFollower
    /// </summary>
    public WaypointFollower GetTargetWaypointFollower()
    {
        return targetWaypointFollower;
    }
}