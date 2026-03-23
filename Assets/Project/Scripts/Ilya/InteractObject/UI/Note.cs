using Akila.FPSFramework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Note : MonoBehaviour
{
    [SerializeField] private string _nameNote;

    public void OpenNote()
    {
        FPSFrameworkCore.IsPaused = true;
        UIManager.Instance.OpenNameNotes(_nameNote);
    }
}
