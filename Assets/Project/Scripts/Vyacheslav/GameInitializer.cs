using UnityEngine;
using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine.UI;
public class GameInitializer : MonoBehaviour
{
    [SerializeField] Inventory _inventory;
    [SerializeField] InGameUI _inGameUI;

    private void Awake()
    {
        if (_inventory == null)
        {
            Debug.LogError("Not found playerInventory");
            return;
        }
       // _inGameUI.Init(_inventory);
    }
}
