using Akila.FPSFramework;
using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private string _nameNote;

    public void OpenNote()
    {
        UIManager.Instance.OpenNameNotes(_nameNote);
    }
}
