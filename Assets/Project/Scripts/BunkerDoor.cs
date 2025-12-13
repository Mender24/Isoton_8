using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunkerDoor : MonoBehaviour
{
    //public Transform pivot_left;
    public Transform pivot_right;
    public float roughness = 2;

    private Quaternion targetRotation;
    public bool isOpenInitially = false;

    private void Start()
    {
        targetRotation = isOpenInitially ? Quaternion.Euler(-90, 180, -90) : Quaternion.identity;   
        //targetRotation = isOpenInitially ? Quaternion.Euler(0, 0, 0) : Quaternion.identity;
    }

    private void Update()
    {
        //pivot_left.localRotation = Quaternion.Lerp(pivot_left.localRotation, targetRotation, Time.deltaTime * roughness);
        pivot_right.localRotation = Quaternion.Lerp(pivot_right.localRotation, Quaternion.Inverse(targetRotation), Time.deltaTime * roughness);
    }

    public void ToggleDoor()
    {
        targetRotation = targetRotation == Quaternion.Euler(0, 90, 0) ? Quaternion.identity : Quaternion.Euler(0, 90, 0);
    }
}
