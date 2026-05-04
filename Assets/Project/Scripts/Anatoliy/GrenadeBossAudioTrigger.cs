using UnityEngine;

public class GrenadeBossAudioTrigger : MonoBehaviour
{
    [SerializeField] private EnemyAudioController _audioController;
    [SerializeField] private string _grenadeBossEnterPhraseName = "GrenadeBossPhrase";
    public bool activateOnce = true;
    private bool _hasActivated = false;

    void Start()
    {
        if (_audioController == null)
        {
            Debug.LogError("No enemy audio was assigned in grenade boss audio trigger on " + gameObject.name);
            _hasActivated = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _hasActivated) return;

        if (other.CompareTag("Player"))
        {
            _audioController.PlayNamedSound(_grenadeBossEnterPhraseName);

            _hasActivated = true;
        }
    }
}
