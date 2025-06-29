using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI: MonoBehaviour
{
    [Header("Basic Inventory")]
    public GameObject inventoryPanel;
    public Player player;
    public List<Slot_UI> slots = new();

    [Header("Sell Mode")]
    public Button sellModeButton;
    public TextMeshProUGUI sellModeText;
    public GameObject sellInfoPanel;
    public TextMeshProUGUI totalValueText;
    public Button sellAllButton;
    
    // State
    private bool sellModeActive = false;
    private ShopManager shopManager;

    // Property để check inventory có đang mở không
    public bool IsOpen => inventoryPanel.activeSelf;

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
        
        // Find ShopManager
        shopManager = FindObjectOfType<ShopManager>();
        
        // Initialize drag drop for inventory slots
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].InitializeDragDrop(SlotType.Inventory, i);
        }
        
        // Setup sell mode UI
        SetupSellMode();
        
        // Initially hide sell info panel
        if (sellInfoPanel != null)
            sellInfoPanel.SetActive(false);
    }
    
    /// <summary>
    /// Setup sell mode buttons and events
    /// </summary>
    private void SetupSellMode()
    {
        if (sellModeButton != null)
        {
            sellModeButton.onClick.AddListener(ToggleSellMode);
        }
        
        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(SellAllItems);
        }
        
        UpdateSellModeUI();
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
        
        // ✅ Update sell info if in sell mode
        if (sellModeActive)
        {
            UpdateSellInfo();
        }
    }

    public void Remove(int slotID)
    {
        var type = player.inventory.slots[slotID].type;
        var itemToDrop = GameManager.instance.itemManager.GetItemByType(type);
        
        if (itemToDrop == null)
        {
            Debug.LogWarning("Không tìm thấy item tương ứng để drop!");
            return;
        }

        player.DropItem(itemToDrop);
        player.inventory.Remove(slotID);
        Refresh();
    }
    
    // ✅ SELL MODE FUNCTIONALITY
    
    /// <summary>
    /// Toggle sell mode on/off
    /// </summary>
    public void ToggleSellMode()
    {
        SetSellMode(!sellModeActive);
    }
    
    /// <summary>
    /// Set sell mode state
    /// </summary>
    public void SetSellMode(bool enabled)
    {
        sellModeActive = enabled;
        
        // Update all slots
        foreach (var slot in slots)
        {
            slot.SetSellMode(enabled);
        }
        
        // Update sell UI
        UpdateSellModeUI();
        
        // Show/hide sell info panel
        if (sellInfoPanel != null)
            sellInfoPanel.SetActive(enabled);
            
        // Update sell info if enabled
        if (enabled)
        {
            UpdateSellInfo();
        }
        
        Debug.Log($"Inventory_UI: Sell mode {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    /// <summary>
    /// Update sell mode UI elements
    /// </summary>
    private void UpdateSellModeUI()
    {
        if (sellModeText != null)
        {
            sellModeText.text = sellModeActive ? "Thoát Chế Độ Bán" : "Chế Độ Bán";
        }
        
        if (sellModeButton != null)
        {
            var colors = sellModeButton.colors;
            colors.normalColor = sellModeActive ? Color.red : Color.green;
            sellModeButton.colors = colors;
        }
    }
    
    /// <summary>
    /// Update sell info display
    /// </summary>
    private void UpdateSellInfo()
    {
        if (!sellModeActive || shopManager == null) return;
        
        float totalValue = 0f;
        int sellableItems = 0;
        
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    totalValue += shopItem.sellPrice * slot.count;
                    sellableItems += slot.count;
                }
            }
        }
        
        if (totalValueText != null)
        {
            totalValueText.text = $"Tổng giá trị: {totalValue}G ({sellableItems} items)";
        }
        
        if (sellAllButton != null)
        {
            sellAllButton.interactable = sellableItems > 0;
        }
    }
    
    /// <summary>
    /// Sell all sellable items
    /// </summary>
    public void SellAllItems()
    {
        if (!sellModeActive || shopManager == null) return;
        
        int totalSold = 0;
        float totalEarned = 0f;
        
        // Process from back to front to avoid index shifting issues
        for (int i = player.inventory.slots.Count - 1; i >= 0; i--)
        {
            var slot = player.inventory.slots[i];
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    int quantityToSell = slot.count;
                    if (shopManager.SellItem(i, quantityToSell))
                    {
                        totalSold += quantityToSell;
                        totalEarned += shopItem.sellPrice * quantityToSell;
                    }
                }
            }
        }
        
        if (totalSold > 0)
        {
            Debug.Log($"Inventory_UI: Sold {totalSold} items for {totalEarned}G total");
            UpdateSellInfo();
        }
        else
        {
            Debug.Log("Inventory_UI: No items were sold");
        }
    }
    

}
