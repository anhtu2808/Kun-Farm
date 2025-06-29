using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI system để bán items từ inventory của player
/// </summary>
public class PlayerInventorySell_UI : MonoBehaviour
{
    [Header("References")]
    public ShopManager shopManager;
    public Player player;
    
    [Header("Sell UI")]
    public GameObject sellPanel;
    public Transform inventoryContainer;
    public Button sellModeButton;
    public TextMeshProUGUI sellModeText;
    
    [Header("Sell Info")]
    public TextMeshProUGUI totalValueText;
    public Button sellAllButton;
    public Button closeSellButton;
    
    // State
    [SerializeField] private bool sellModeActive = false;
    
    /// <summary>
    /// Public property để check sell mode state
    /// </summary>
    public bool SellModeActive => sellModeActive;
    
    private void Awake()
    {
        // Auto-find references if not assigned
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
            
        if (player == null)
            player = FindObjectOfType<Player>();
            
        // Setup button events
        if (sellModeButton != null)
            sellModeButton.onClick.AddListener(ToggleSellMode);
            
        if (sellAllButton != null)
            sellAllButton.onClick.AddListener(SellAllItems);
            
        if (closeSellButton != null)
            closeSellButton.onClick.AddListener(() => SetSellMode(false));
    }
    
    private void Start()
    {
        // Ensure sell panel is hidden initially
        if (sellPanel != null)
            sellPanel.SetActive(false);
            
        UpdateSellModeUI();
    }
    
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
        
        if (sellPanel != null)
            sellPanel.SetActive(enabled);
            
        UpdateSellModeUI();
        
        if (enabled)
        {
            RefreshInventoryDisplay();
        }
        
        Debug.Log($"PlayerInventorySell_UI: Sell mode {(enabled ? "ENABLED" : "DISABLED")}");
    }
    
    /// <summary>
    /// Update sell mode button text
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
    /// Refresh inventory display with sell prices
    /// </summary>
    private void RefreshInventoryDisplay()
    {
        if (player == null || player.inventory == null) return;
        
        float totalValue = 0f;
        int sellableItems = 0;
        
        // Calculate total value of sellable items
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager?.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    totalValue += shopItem.sellPrice * slot.count;
                    sellableItems += slot.count;
                }
            }
        }
        
        // Update UI
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
    /// Sell specific item from inventory slot
    /// </summary>
    public bool SellItemFromSlot(CollectableType itemType, int quantity = 1)
    {
        if (shopManager == null || player == null) return false;
        
        // Check if player has the item
        int playerItemCount = GetPlayerItemCount(itemType);
        if (playerItemCount < quantity) return false;
        
        // Check if item can be sold
        ShopItem shopItem = shopManager.shopData?.GetShopItem(itemType);
        if (shopItem == null || !shopItem.canSell) return false;
        
        // Find the inventory slot index with this item type
        int slotIndex = FindInventorySlotIndex(itemType);
        if (slotIndex == -1) return false;
        
        // Sell the item using slot index
        bool success = shopManager.SellItem(slotIndex, quantity);
        if (success)
        {
            RefreshInventoryDisplay();
            Debug.Log($"PlayerInventorySell_UI: Sold {quantity}x {shopItem.itemName} for {shopItem.sellPrice * quantity}G");
        }
        
        return success;
    }
    
    /// <summary>
    /// Sell all sellable items in inventory
    /// </summary>
    public void SellAllItems()
    {
        if (player == null || player.inventory == null) return;
        
        int totalSold = 0;
        float totalEarned = 0f;
        
        // Process from back to front to avoid index shifting issues
        for (int i = player.inventory.slots.Count - 1; i >= 0; i--)
        {
            var slot = player.inventory.slots[i];
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager?.shopData?.GetShopItem(slot.type);
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
            Debug.Log($"PlayerInventorySell_UI: Sold {totalSold} items for {totalEarned}G total");
            RefreshInventoryDisplay();
        }
        else
        {
            Debug.Log("PlayerInventorySell_UI: No items were sold");
        }
    }
    
    /// <summary>
    /// Get count of specific item in player inventory
    /// </summary>
    private int GetPlayerItemCount(CollectableType itemType)
    {
        if (player == null || player.inventory == null) return 0;
        
        int count = 0;
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == itemType)
                count += slot.count;
        }
        return count;
    }
    
    /// <summary>
    /// Get sell price for an item
    /// </summary>
    public int GetSellPrice(CollectableType itemType)
    {
        ShopItem shopItem = shopManager?.shopData?.GetShopItem(itemType);
        return shopItem != null && shopItem.canSell ? shopItem.sellPrice : 0;
    }
    
    /// <summary>
    /// Check if item can be sold
    /// </summary>
    public bool CanSellItem(CollectableType itemType)
    {
        ShopItem shopItem = shopManager?.shopData?.GetShopItem(itemType);
        return shopItem != null && shopItem.canSell;
    }
    
    /// <summary>
    /// Find inventory slot index for a specific item type
    /// </summary>
    private int FindInventorySlotIndex(CollectableType itemType)
    {
        if (player == null || player.inventory == null) return -1;
        
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == itemType && slot.count > 0)
            {
                return i;
            }
        }
        return -1;
    }
    
    private void Update()
    {
        // Auto-refresh if sell mode is active
        if (sellModeActive && Time.frameCount % 30 == 0) // Every 30 frames
        {
            RefreshInventoryDisplay();
        }
    }
}