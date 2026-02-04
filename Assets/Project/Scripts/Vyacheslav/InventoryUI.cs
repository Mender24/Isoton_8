using UnityEngine;
using Akila.FPSFramework;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    [SerializeField] private List<Image> _slots;

    [SerializeField] private float _showDuration = 2f;
    private List<InventoryViewNote> _viewNotes = new List<InventoryViewNote>();
    private float _endShowTime;
    protected int CurrentInventorySize => _viewNotes.Count - 1;
    protected int LastInventoryIndex => _viewNotes.Count;
    private bool _isActive;

    public void Init(Inventory inventory)
    {
        inventory.OnChangeWeapone += UpdateWeaponeCurrent;
        inventory.OnAddWeapone += AddWeapone;
        UpdateWeaponeAll(-1);
        SetActive(false);
    }

    protected void Update()
    {
        TryHideInventory();
    }

    private void TryHideInventory()
    {
        if (!_isActive)
        {
            return;
        }
        if (Time.time > _endShowTime)
        {
            HideInventory();
        }
    }

    private void UpdateWeaponeCurrent(int index)
    {
        if (index >= _viewNotes.Count)
        {
            index = _viewNotes.Count - 1;
        }
        UpdateWeaponeAll(index);
    }

    private void AddWeapone(InventoryItem weapone)
    {
        _viewNotes.Add(new InventoryViewNote(weapone));
        UpdateWeaponeAll(_viewNotes.Count-1);
    }

    private void UpdateWeaponeAll(int index)
    {
        var iterator = 0;
        foreach (var slot in _slots)
        {
            if (iterator >= _viewNotes.Count)
            {
                slot.gameObject.SetActive(false);
            }
            else
            {
                slot.sprite = iterator == index ? _viewNotes[iterator].ActiveSprite : _viewNotes[iterator].NoActiveSprite;
                slot.gameObject.SetActive(true);
            }
            iterator++;
        }
        SetActive(true);
    }

    private void ShowWeaponeAll()
    {
        this.gameObject.SetActive(true);
        _endShowTime = Time.time + _showDuration;

    }

    private void HideInventory()
    {
        SetActive(false);
    }

    private void SetActive(bool value)
    {
        this.gameObject.SetActive(value);
        _isActive = value;
        if (true)
        {
            _endShowTime = Time.time + _showDuration;
        }
    }
}

public class InventoryViewNote
{
    public Sprite ActiveSprite;
    public Sprite NoActiveSprite;
    public InventoryViewNote (InventoryItem item)
    {
        NoActiveSprite = item.UISpriteDeactivated;
        ActiveSprite = item.UISpriteActive;
    }
}
