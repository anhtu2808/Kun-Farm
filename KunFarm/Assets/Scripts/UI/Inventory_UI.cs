// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class InventoryUI: MonoBehaviour
// {
//     [Header("Basic Inventory")]
//     public GameObject inventoryPanel;
//     public Player player;
//     public List<Slot_UI> slots = new();
   

//     // Property để check inventory có đang mở không
//     public bool IsOpen => inventoryPanel.activeSelf;

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Tab))
//         {
//             ToggleInventory();
//         }
//     }

//     public void ToggleInventory()
//     {
//         if (!inventoryPanel.activeSelf)
//         {
//             inventoryPanel.SetActive(true);
//             Refresh();
//         }
//         else
//         {
//             inventoryPanel.SetActive(false);
//         }
//     }

//     void Start()
//     {
//         inventoryPanel.SetActive(false);
//         player.inventory.onInventoryChanged += Refresh;
        
//         // Initialize drag drop for inventory slots
//         for (int i = 0; i < slots.Count; i++)
//         {
//             slots[i].InitializeDragDrop(SlotType.Inventory, i);
//         }
    
//     }
    

//     public void Refresh()
//     {
//         if (slots.Count == player.inventory.slots.Count)
//         {
//             for (int i = 0; i < slots.Count; i++)
//             {
//                 if (player.inventory.slots[i].type != CollectableType.NONE)
//                 {
//                     slots[i].SetItem(player.inventory.slots[i]);
//                 }
//                 else
//                 {
//                     slots[i].SetEmpty();
//                 }
//             }
//         }
//     }

//     public void Remove(int slotID)
//     {
//         var type = player.inventory.slots[slotID].type;
//         var itemToDrop = GameManager.instance.itemManager.GetItemByType(type);
        
//         if (itemToDrop == null)
//         {
//             Debug.LogWarning("Không tìm thấy item tương ứng để drop!");
//             return;
//         }

//         player.DropItem(itemToDrop);
//         player.inventory.Remove(slotID);
//         Refresh();
//     }
// }
