using Akila.FPSFramework;
using UnityEngine;

public class SoundTransition : MonoBehaviour
{
    private bool _isOut = false;

    private void OnTriggerStay(Collider other)
    {
        if (SoundManager.Instance == null)
            return;

        if(other.TryGetComponent(out Player player))
        {
            if(!_isOut)
                SoundManager.Instance.TransitionIn();

            _isOut = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (SoundManager.Instance == null)
            return;

        if (other.TryGetComponent(out Player player))
        {
            if (_isOut)
                SoundManager.Instance.TransitionOut();

            _isOut = false;
        }
    }
}
