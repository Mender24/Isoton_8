using UnityEngine;
using System.Collections.Generic;
using Akila.FPSFramework;

public class FreezeZone : MonoBehaviour
{
    [SerializeField] private float _duration=5f;
    private float _endTime;
    private List<FreezeNote> _freezeNotes = new List<FreezeNote>();


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            var freezed = other.gameObject.GetComponent<Akila.FPSFramework.IFreezed>();
            var speed = freezed.Freeze();
            _freezeNotes.Add(new FreezeNote(freezed, speed));
        }
    }

    public void Dectivate()
    {
        foreach (var note in _freezeNotes)
        {
            note.freezed.UnFreeze(note.speed);
        }
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        _endTime = Time.time + _duration;
    }

    private void Update()
    {
        if (Time.time > _endTime)
        {
            Dectivate();
        }
    }
}

public class FreezeNote
{
    public IFreezed freezed;
    public Vector3 speed;

    public FreezeNote(IFreezed freezed, Vector3 speed)
    {
        this.freezed = freezed;
        this.speed = speed;
    }
}




