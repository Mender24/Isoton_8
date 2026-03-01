using Akila.FPSFramework;
using UnityEngine;

public class SoundTriggerRandomAudio : MonoBehaviour
{
    [SerializeField] private bool _isOnePlay = true;
    [SerializeField] private string _soundName;

    private BoxCollider _boxCollider;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            SoundManager.Instance.PlayScriptedSoundName(_soundName);

            if (_boxCollider == null)
                _boxCollider = GetComponent<BoxCollider>();

            if(_isOnePlay)
                _boxCollider.enabled = false;
        }
    }
}
