using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunkerDoor : MonoBehaviour
{
    //public Transform pivot_left;
    public Transform pivot_hinge;
    public float roughness = 2;

    private Quaternion targetRotation;
    public bool isOpenInitially = false;

    //private void Start()
    //{
    //    Debug.Log("Start");
    //    if(isOpenInitially)
    //        targetRotation = Quaternion.Euler(0, -90, 0);   
    //}

    private void Update()
    {
        if(targetRotation != pivot_hinge.localRotation)
            pivot_hinge.localRotation = Quaternion.Lerp(pivot_hinge.localRotation, Quaternion.Inverse(targetRotation), Time.deltaTime * roughness);
    }

    public void ToggleDoor()
    {
        targetRotation = targetRotation == Quaternion.Euler(0, -90, 0) ? Quaternion.identity : Quaternion.Euler(0, -90, 0);
    }

    public void OpenDoor()
    {
        targetRotation = Quaternion.Euler(0, -90, 0);
    }

    public void CloseDoor()
    {
        targetRotation = Quaternion.identity;
    }
}
