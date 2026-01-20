using UnityEngine;
using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private List<Image> _weapones;

    public void Init(Inventory inventory)
    {
        inventory.OnChangeWeapone += UpdateWeaponeCurrent;
        inventory.OnAddWeapone += UpdateWeaponeAll;
    }

    protected void Start()
    {
        Init(_inventory);
        UpdateWeaponeAll(-1, null);
    }

    private void UpdateWeaponeCurrent(int index)
    {

    }

    private void UpdateWeaponeAll(int index, InventoryItem weapone)
    {
        int i = 0;
        foreach (var item in _weapones)
        {
            item.gameObject.SetActive(i < index);
            if (i==index-1)
            {
                item.sprite = weapone.UISprite;
            }
            i++;
        }
    }
}
