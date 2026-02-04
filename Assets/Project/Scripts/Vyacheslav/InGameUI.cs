using UnityEngine;
using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Inventory _inventory;


    private void Start()
    {
        Init();
    }
    private void Init()
    {
        _inventoryUI.Init(_inventory);
    }

   
}
