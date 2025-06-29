using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Extension cho InventorySlot để thêm khả năng bán items
/// </summary>
[RequireComponent(typeof(Button))]
public class InventorySlot_Sellable : MonoBehaviour
{
    [Header("Sell UI")]
    public GameObject sellOverlay;
    public TextMeshProUGUI sellPriceText;
    public Button sellButton;
    public TextMeshProUGUI sellButtonText;
    
    [Header("Visual")]
    public Color sellModeColor = Color.yellow;
    public Color normalColor = Color.white;
    
    // References
    private PlayerInventorySell_UI sellSystem;
    private Slot_UI inventorySlot;
    private Image slotImage;
    private bool isInSellMode = false;
    
    // Current slot data
    private CollectableType currentItemType = CollectableType.NONE;
    private int currentItemCount = 0;
    
    private void Awake()
    {
        // Get components
        inventorySlot = GetComponent<Slot_UI>();
        slotImage = GetComponent<Image>();
        
        // Find sell system
        sellSystem = FindObjectOfType<PlayerInventorySell_UI>();
        
        // Setup sell button
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }
        
        // Setup main slot button for sell mode
        Button mainButton = GetComponent<Button>();
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(OnSlotClicked);
        }
        
        // Initially hide sell overlay
        if (sellOverlay != null)
            sellOverlay.SetActive(false);
    }
    
    private void Start()
    {
        // Subscribe to sell mode changes
        if (sellSystem != null)
        {
            // Tạo event simple bằng cách check trong Update
        }
    }
    
    private void Update()
    {
        // Check if sell mode changed
        bool sellModeActive = sellSystem != null && sellSystem.SellModeActive;
        
        if (sellModeActive != isInSellMode)
        {
            SetSellMode(sellModeActive);
        }
        
        // Update slot data if changed - only check every few frames for performance
        if (inventorySlot != null && Time.frameCount % 10 == 0)
        {
            var slotData = inventorySlot.GetCurrentSlot();
            CollectableType newType = slotData?.type ?? CollectableType.NONE;
            int newCount = slotData?.count ?? 0;
            
            // Only update if data actually changed
            if (newType != currentItemType || newCount != currentItemCount)
            {
                UpdateSlotData();
                
                // Update sell display if in sell mode
                if (isInSellMode)
                {
                    UpdateSellPriceDisplay();
                }
            }
        }
    }
    
    /// <summary>
    /// Update current slot data
    /// </summary>
    private void UpdateSlotData()
    {
        // Get slot data from Slot_UI component
        if (inventorySlot != null)
        {
            var slotData = inventorySlot.GetCurrentSlot();
            if (slotData != null)
            {
                currentItemType = slotData.type;
                currentItemCount = slotData.count;
            }
            else
            {
                currentItemType = CollectableType.NONE;
                currentItemCount = 0;
            }
        }
    }
    
    /// <summary>
    /// Set slot to sell mode
    /// </summary>
    private void SetSellMode(bool enabled)
    {
        isInSellMode = enabled;
        
        // Update visual
        if (slotImage != null)
        {
            slotImage.color = enabled ? sellModeColor : normalColor;
        }
        
        // Show/hide sell overlay
        if (sellOverlay != null)
        {
            sellOverlay.SetActive(enabled && CanSellCurrentItem());
        }
        
        // Update sell price text
        UpdateSellPriceDisplay();
    }
    
    /// <summary>
    /// Update sell price display
    /// </summary>
    private void UpdateSellPriceDisplay()
    {
        if (sellPriceText != null && sellSystem != null)
        {
            if (CanSellCurrentItem())
            {
                int sellPrice = sellSystem.GetSellPrice(currentItemType);
                sellPriceText.text = $"Bán: {sellPrice}G";
                sellPriceText.gameObject.SetActive(true);
            }
            else
            {
                sellPriceText.gameObject.SetActive(false);
            }
        }
        
        // Update sell button
        if (sellButton != null)
        {
            sellButton.interactable = CanSellCurrentItem();
            
            if (sellButtonText != null)
            {
                sellButtonText.text = currentItemCount > 1 ? $"Bán 1/{currentItemCount}" : "Bán";
            }
        }
    }
    
    /// <summary>
    /// Check if current item can be sold
    /// </summary>
    private bool CanSellCurrentItem()
    {
        return currentItemType != CollectableType.NONE && 
               currentItemCount > 0 && 
               sellSystem != null && 
               sellSystem.CanSellItem(currentItemType);
    }
    
    /// <summary>
    /// Handle slot click (for sell mode)
    /// </summary>
    private void OnSlotClicked()
    {
        if (isInSellMode && CanSellCurrentItem())
        {
            SellOneItem();
        }
    }
    
    /// <summary>
    /// Handle sell button click
    /// </summary>
    private void OnSellButtonClicked()
    {
        if (CanSellCurrentItem())
        {
            SellOneItem();
        }
    }
    
    /// <summary>
    /// Sell one item from this slot
    /// </summary>
    private void SellOneItem()
    {
        if (sellSystem != null && CanSellCurrentItem())
        {
            bool success = sellSystem.SellItemFromSlot(currentItemType, 1);
            if (success)
            {
                Debug.Log($"InventorySlot_Sellable: Sold 1x {currentItemType}");
                
                // Update display after selling
                UpdateSlotData();
                UpdateSellPriceDisplay();
            }
        }
    }
    
    /// <summary>
    /// Set item data manually (for integration)
    /// </summary>
    public void SetItemData(CollectableType itemType, int itemCount)
    {
        currentItemType = itemType;
        currentItemCount = itemCount;
        
        UpdateSellPriceDisplay();
    }
    
    /// <summary>
    /// Get current item type
    /// </summary>
    public CollectableType GetItemType()
    {
        return currentItemType;
    }
    
    /// <summary>
    /// Get current item count
    /// </summary>
    public int GetItemCount()
    {
        return currentItemCount;
    }
} 