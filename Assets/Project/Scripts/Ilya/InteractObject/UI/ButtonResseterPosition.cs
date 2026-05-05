using Akila.FPSFramework;
using UnityEngine;

public class ButtonResseterPosition : MonoBehaviour
{
    public void ResetPosition()
    {
        if(Player.Instance != null)
        {
            Player.Instance.ActivateEventResetPosition();
        }
    }
}
