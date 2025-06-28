using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI: MonoBehaviour
{
    public GameObject inventoryPanel;
    public Player player;
    public List<Slot_UI> slots = new();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            Refresh();
        }
        else
        {
            inventoryPanel.SetActive(false);
        }
    }

    void Start()
    {
        inventoryPanel.SetActive(false);
        player.inventory.onInventoryChanged += Refresh;
        
        // Initialize drag drop for inventory slots
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].InitializeDragDrop(SlotType.Inventory, i);
        }
    }

    public void Refresh()
    {
        if (slots.Count == player.inventory.slots.Count)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (player.inventory.slots[i].type != CollectableType.NONE)
                {
                    slots[i].SetItem(player.inventory.slots[i]);
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }
        }
    }

    public void Remove(int slotID)
    {
        Debug.Log("Remove() được gọi với slotID: " + slotID);
        var type = player.inventory.slots[slotID].type;
        Debug.Log("Loại vật phẩm trong slot là: " + type);

        var itemToDrop = GameManager.instance.itemManager.GetItemByType(type);
        if (itemToDrop == null)
        {
            Debug.LogWarning("Không tìm thấy item tương ứng để drop!");
            return;
        }

        Debug.Log("Chuẩn bị gọi DropItem với: " + itemToDrop.name);
        player.DropItem(itemToDrop);
        player.inventory.Remove(slotID);
        Refresh();
    }
}
