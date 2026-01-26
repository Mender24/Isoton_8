using UnityEngine;
using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private InventoryUI _inventoryUI;


    public void Init(Inventory inventory)
    {
        _inventoryUI.Init(inventory);
    }

   
}
