using UnityEngine;

public class OpenVaultDoorAnimator : MonoBehaviour
{
    [SerializeField] Animator bunkerDoorAnimator;
    [SerializeField] Animator bunkerHandAnimator;

    public void OpenDoor()
    {
        bunkerDoorAnimator.SetTrigger("OpenDoor");
        bunkerHandAnimator.SetTrigger("OpenDoor");
    }

}
