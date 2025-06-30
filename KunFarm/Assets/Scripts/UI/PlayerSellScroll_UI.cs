using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// UI Manager cho PlayerSell_Scroll - hiển thị inventory items của player với sell functionality
/// </summary>
public class PlayerSellScroll_UI : MonoBehaviour
{
    [Header("References")]
    public ShopManager shopManager;
    public Player player;
    public Transform playerItemsContainer; // Player_Items container
    
    [Header("Item Slot Structure")]
    [Tooltip("Prefab structure: Item/icon(Image), Item/sellbutton(Button), Item/price(TextMeshProUGUI), Item/quantity(TextMeshProUGUI)")]
    public List<PlayerSellItem> itemSlots = new List<PlayerSellItem>();
    
    [Header("Sell All Button")]
    public Button sellAllButton;
    public TextMeshProUGUI sellAllText;
    
    [System.Serializable]
    public class PlayerSellItem
    {
        public GameObject itemObject;
        public Image iconImage;
        public Button sellButton;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI quantityText;
        public CollectableType currentItemType = CollectableType.NONE;
        public int currentItemCount = 0;
        public int currentItemPrice = 0;
    }
    
    // Events
    public event Action<CollectableType, int, int> OnItemSold; // itemType, quantity, earnings
    
    private void Awake()
    {
        // Auto-find references nếu chưa gán
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
            
        if (player == null)
            player = FindObjectOfType<Player>();
            
        // Tìm Player_Items container nếu chưa gán
        if (playerItemsContainer == null)
        {
            playerItemsContainer = transform.Find("Player_Items");
            if (playerItemsContainer == null)
            {
                Debug.LogError("PlayerSellScroll_UI: Không tìm thấy Player_Items container!");
                return;
            }
        }
        
        InitializeItemSlots();
        SetupSellAllButton();
    }
    
    private void Start()
    {
        RefreshDisplay();
    }
    
    /// <summary>
    /// Initialize với references
    /// </summary>
    public void Initialize(ShopManager manager, Player playerRef)
    {
        shopManager = manager;
        player = playerRef;
        RefreshDisplay();
    }
    
    /// <summary>
    /// Khởi tạo các item slots từ children của Player_Items
    /// </summary>
    private void InitializeItemSlots()
    {
        itemSlots.Clear();
        
        // Lấy tất cả children của Player_Items
        for (int i = 0; i < playerItemsContainer.childCount; i++)
        {
            Transform itemTransform = playerItemsContainer.GetChild(i);
            GameObject itemObject = itemTransform.gameObject;
            
            // Tìm components trong item
            Image iconImage = null;
            Button sellButton = null;
            TextMeshProUGUI priceText = null;
            TextMeshProUGUI quantityText = null;
            
            // Tìm icon (có thể có nested structure)
            iconImage = itemTransform.GetComponentInChildren<Image>();
            if (iconImage == itemObject.GetComponent<Image>()) // Skip background image
            {
                Image[] images = itemTransform.GetComponentsInChildren<Image>();
                iconImage = images.Length > 1 ? images[1] : images[0];
            }
            
            // Tìm sell button
            Button[] buttons = itemTransform.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("sell") || btn.name.ToLower().Contains("button"))
                {
                    sellButton = btn;
                    break;
                }
            }
            
            // Tìm price text
            TextMeshProUGUI[] texts = itemTransform.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI txt in texts)
            {
                if (txt.name.ToLower().Contains("price"))
                {
                    priceText = txt;
                }
                else if (txt.name.ToLower().Contains("quantity") || txt.name.ToLower().Contains("count"))
                {
                    quantityText = txt;
                }
            }
            
            // Tạo PlayerSellItem
            PlayerSellItem sellItem = new PlayerSellItem
            {
                itemObject = itemObject,
                iconImage = iconImage,
                sellButton = sellButton,
                priceText = priceText,
                quantityText = quantityText
            };
            
            itemSlots.Add(sellItem);
            
            // Setup sell button event
            if (sellButton != null)
            {
                int slotIndex = i; // Capture index for closure
                sellButton.onClick.AddListener(() => OnSellButtonClicked(slotIndex));
            }
            
            // Initially hide item
            itemObject.SetActive(false);
        }
        
        Debug.Log($"PlayerSellScroll_UI: Initialized {itemSlots.Count} item slots");
    }
    
    /// <summary>
    /// Setup sell all button
    /// </summary>
    private void SetupSellAllButton()
    {
        if (sellAllButton != null)
        {
            sellAllButton.onClick.AddListener(SellAllItems);
        }
    }
    
    /// <summary>
    /// Refresh display với inventory items
    /// </summary>
    public void RefreshDisplay()
    {
        if (player == null || player.inventory == null || shopManager == null || shopManager.shopData == null)
        {
            Debug.LogWarning("PlayerSellScroll_UI: Missing references for refresh");
            return;
        }
        
        // Lấy danh sách items từ inventory
        List<Inventory.Slot> inventoryItems = new List<Inventory.Slot>();
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                inventoryItems.Add(slot);
            }
        }
        
        // Cập nhật từng slot
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < inventoryItems.Count)
            {
                // Có item để hiển thị
                UpdateSlot(i, inventoryItems[i]);
            }
            else
            {
                // Slot trống
                ClearSlot(i);
            }
        }
        
        // Update sell all button
        UpdateSellAllButton();
        
        Debug.Log($"PlayerSellScroll_UI: Refreshed display with {inventoryItems.Count} items");
    }
    
    /// <summary>
    /// Cập nhật một slot với item data
    /// </summary>
    private void UpdateSlot(int slotIndex, Inventory.Slot inventorySlot)
    {
        if (slotIndex >= itemSlots.Count) return;
        
        PlayerSellItem sellItem = itemSlots[slotIndex];
        sellItem.currentItemType = inventorySlot.type;
        sellItem.currentItemCount = inventorySlot.count;
        
        // Lấy shop item data
        ShopItem shopItem = shopManager.shopData.GetShopItem(inventorySlot.type);
        
        // Cập nhật icon
        if (sellItem.iconImage != null && shopItem != null)
        {
            sellItem.iconImage.sprite = shopItem.itemIcon;
            sellItem.iconImage.color = Color.white;
            sellItem.iconImage.gameObject.SetActive(true);
        }
        
        // Cập nhật quantity text
        if (sellItem.quantityText != null)
        {
            sellItem.quantityText.text = $"x{inventorySlot.count}";
            sellItem.quantityText.gameObject.SetActive(true);
        }
        
        // Cập nhật price text
        if (sellItem.priceText != null)
        {
            if (shopItem != null && shopItem.canSell)
            {
                int totalValue = shopItem.sellPrice * inventorySlot.count;
                sellItem.currentItemPrice = totalValue;
                sellItem.priceText.text = $"{totalValue}G";
                sellItem.priceText.color = Color.yellow;
            }
            else
            {
                sellItem.currentItemPrice = 0;
                sellItem.priceText.text = "Không bán được";
                sellItem.priceText.color = Color.red;
            }
            sellItem.priceText.gameObject.SetActive(true);
        }
        
        // Cập nhật sell button
        if (sellItem.sellButton != null)
        {
            bool canSell = shopItem != null && shopItem.canSell;
            sellItem.sellButton.interactable = canSell;
            sellItem.sellButton.gameObject.SetActive(true);
            
            // Cập nhật text của button nếu có
            TextMeshProUGUI buttonText = sellItem.sellButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canSell ? "Bán" : "Không thể bán";
            }
        }
        
        // Show item object
        sellItem.itemObject.SetActive(true);
    }
    
    /// <summary>
    /// Clear một slot
    /// </summary>
    private void ClearSlot(int slotIndex)
    {
        if (slotIndex >= itemSlots.Count) return;
        
        PlayerSellItem sellItem = itemSlots[slotIndex];
        sellItem.currentItemType = CollectableType.NONE;
        sellItem.currentItemCount = 0;
        sellItem.currentItemPrice = 0;
        
        // Hide all UI elements
        if (sellItem.iconImage != null)
            sellItem.iconImage.gameObject.SetActive(false);
        if (sellItem.priceText != null)
            sellItem.priceText.gameObject.SetActive(false);
        if (sellItem.quantityText != null)
            sellItem.quantityText.gameObject.SetActive(false);
        if (sellItem.sellButton != null)
            sellItem.sellButton.gameObject.SetActive(false);
            
        // Hide item object
        sellItem.itemObject.SetActive(false);
    }
    
    /// <summary>
    /// Handle sell button click
    /// </summary>
    private void OnSellButtonClicked(int slotIndex)
    {
        SellItem(slotIndex);
    }
    
    /// <summary>
    /// Bán item tại slot
    /// </summary>
    private void SellItem(int slotIndex)
    {
        if (slotIndex >= itemSlots.Count) return;
        
        PlayerSellItem sellItem = itemSlots[slotIndex];
        
        if (sellItem.currentItemType == CollectableType.NONE || sellItem.currentItemCount <= 0)
        {
            Debug.LogWarning("PlayerSellScroll_UI: No item to sell in slot " + slotIndex);
            return;
        }
        
        // Store item info before clearing
        CollectableType itemTypeToSell = sellItem.currentItemType;
        int quantityToSell = sellItem.currentItemCount;
        
        // Find the actual inventory slot index for this item type
        int inventorySlotIndex = FindInventorySlotIndex(sellItem.currentItemType);
        if (inventorySlotIndex == -1)
        {
            Debug.LogWarning($"PlayerSellScroll_UI: Could not find inventory slot for {sellItem.currentItemType}");
            return;
        }
        
        // Clear slot immediately to prevent double-click issues
        ClearSlot(slotIndex);
        
        // Bán toàn bộ stack
        bool success = shopManager.SellItem(inventorySlotIndex, quantityToSell);
        
        if (success)
        {
            ShopItem shopItem = shopManager.shopData.GetShopItem(itemTypeToSell);
            int totalEarned = shopItem.sellPrice * quantityToSell;
            
            Debug.Log($"PlayerSellScroll_UI: Sold {quantityToSell}x {itemTypeToSell} for {totalEarned}G");
            
            // Trigger event
            OnItemSold?.Invoke(itemTypeToSell, quantityToSell, totalEarned);
            
            // Refresh display sau khi bán (with delay to avoid race condition)
            StartCoroutine(RefreshDisplayWithDelay());
        }
        else
        {
            Debug.LogWarning($"PlayerSellScroll_UI: Failed to sell {itemTypeToSell}");
            // Restore slot if sell failed
            RefreshDisplay();
        }
    }
    
    /// <summary>
    /// Refresh display with small delay to avoid race conditions
    /// </summary>
    private System.Collections.IEnumerator RefreshDisplayWithDelay()
    {
        yield return new WaitForEndOfFrame();
        RefreshDisplay();
    }
    
    /// <summary>
    /// Bán tất cả items có thể bán
    /// </summary>
    public void SellAllItems()
    {
        if (player == null || player.inventory == null) return;
        
        int totalSold = 0;
        int totalEarned = 0;
        
        // Bán tất cả items - process from back to front to avoid index shifting
        for (int i = player.inventory.slots.Count - 1; i >= 0; i--)
        {
            var slot = player.inventory.slots[i];
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager?.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    if (shopManager.SellItem(i, slot.count))
                    {
                        totalSold += slot.count;
                        totalEarned += shopItem.sellPrice * slot.count;
                        
                        // Trigger event for each item sold
                        OnItemSold?.Invoke(slot.type, slot.count, shopItem.sellPrice * slot.count);
                    }
                }
            }
        }
        
        if (totalSold > 0)
        {
            Debug.Log($"PlayerSellScroll_UI: Sold {totalSold} items for {totalEarned}G total");
            RefreshDisplay();
        }
        else
        {
            Debug.Log("PlayerSellScroll_UI: No items were sold");
        }
    }
    
    /// <summary>
    /// Update sell all button state
    /// </summary>
    private void UpdateSellAllButton()
    {
        if (sellAllButton == null) return;
        
        bool hasSellableItems = false;
        int totalValue = 0;
        
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager?.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    hasSellableItems = true;
                    totalValue += shopItem.sellPrice * slot.count;
                }
            }
        }
        
        sellAllButton.interactable = hasSellableItems;
        
        if (sellAllText != null)
        {
            if (hasSellableItems)
            {
                sellAllText.text = $"Bán Tất Cả ({totalValue}G)";
            }
            else
            {
                sellAllText.text = "Không có gì để bán";
            }
        }
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
    
    /// <summary>
    /// Lấy thông tin item tại slot
    /// </summary>
    public PlayerSellItem GetSlotInfo(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < itemSlots.Count)
            return itemSlots[slotIndex];
        return null;
    }
} 