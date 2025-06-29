using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot_UI : MonoBehaviour, IPointerClickHandler
{
    [Header("Basic UI")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    [SerializeField] private GameObject highlight;

    [Header("Sell Mode UI")]
    public GameObject sellOverlay;
    public TextMeshProUGUI sellPriceText;
    public Button sellButton;
    public Image slotBackground;

    [Header("Sell Mode Colors")]
    public Color normalColor = Color.white;
    public Color sellModeColor = Color.yellow;
    public Color sellableItemColor = Color.green;

    private DragDropHandler dragDropHandler;
    private System.Action<int> onSlotClicked;
    private int slotIndex;

    // Sell functionality
    private bool isInSellMode = false;
    private Inventory.Slot currentSlot;
    private ShopManager shopManager;

    void Awake()
    {
        // Add DragDropHandler if not present
        dragDropHandler = GetComponent<DragDropHandler>();
        if (dragDropHandler == null)
        {
            dragDropHandler = gameObject.AddComponent<DragDropHandler>();
            dragDropHandler.itemIcon = itemIcon;
            dragDropHandler.quantityText = quantityText;
        }

        // Find ShopManager for sell functionality
        shopManager = FindObjectOfType<ShopManager>();

        // Setup sell button
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClick);
        }

        // Get slot background if not assigned
        if (slotBackground == null)
        {
            slotBackground = GetComponent<Image>();
        }

        // Initially hide sell overlay
        SetSellOverlayVisible(false);
    }

    public void SetItem(Inventory.Slot slot)
    {
        currentSlot = slot; // ✅ Store for sell functionality

        if (slot != null)
        {
            itemIcon.sprite = slot.icon;
            itemIcon.color = new Color(1, 1, 1, 1);
            quantityText.text = slot.count.ToString();

            // Update drag drop handler
            if (dragDropHandler != null)
                dragDropHandler.SetSlotData(slot);
        }

        // Update sell display if in sell mode
        if (isInSellMode)
        {
            UpdateSellDisplay();
        }
    }

    public void SetTool(Tool tool)
    {
        if (tool != null)
        {
            itemIcon.sprite = tool.toolIcon;
            itemIcon.color = new Color(1, 1, 1, 1);

            // Show quantity cho tools có quantity > 1 và consumable
            if (tool.IsConsumable() && tool.quantity > 1)
            {
                quantityText.text = tool.quantity.ToString();
            }
            else
            {
                quantityText.text = ""; // Hand tool hoặc quantity = 1
            }

            // Update drag drop handler
            if (dragDropHandler != null)
                dragDropHandler.SetToolData(tool);
        }
    }

    public void SetEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0);
        quantityText.text = "";

        // Force clear drag drop handler data
        if (dragDropHandler != null)
        {
            dragDropHandler.SetSlotData(null);
            dragDropHandler.SetToolData(null);
        }
    }

    public void SetHighlight(bool isOn)
    {
        highlight.SetActive(isOn);
    }

    public void InitializeDragDrop(SlotType slotType, int index)
    {
        if (dragDropHandler != null)
            dragDropHandler.Initialize(slotType, index);

        // Store slot index for click handling
        slotIndex = index;
    }

    public void SetClickCallback(System.Action<int> callback)
    {
        onSlotClicked = callback;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle sell mode clicks
        if (isInSellMode && eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            if (CanSellCurrentItem())
            {
                SellOneItem();
                return;
            }
        }

        // Normal click handling (only if not in sell mode)
        if (!isInSellMode && eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            onSlotClicked?.Invoke(slotIndex);
        }
    }

    // ✅ SELL FUNCTIONALITY METHODS

    /// <summary>
    /// Set sell mode for this slot
    /// </summary>
    public void SetSellMode(bool enabled)
    {
        isInSellMode = enabled;
        UpdateSellDisplay();
    }

    /// <summary>
    /// Update sell display based on current state
    /// </summary>
    private void UpdateSellDisplay()
    {
        if (!isInSellMode)
        {
            // Normal mode - hide sell overlay and reset colors
            SetSellOverlayVisible(false);
            SetSlotBackgroundColor(normalColor);
            return;
        }

        // Sell mode active
        bool canSell = CanSellCurrentItem();

        // Update background color
        if (canSell)
        {
            SetSlotBackgroundColor(sellableItemColor);
        }
        else
        {
            SetSlotBackgroundColor(sellModeColor);
        }

        // Update sell overlay
        SetSellOverlayVisible(canSell);

        // Update sell price text
        if (canSell && sellPriceText != null)
        {
            int sellPrice = GetSellPrice();
            sellPriceText.text = $"Bán: {sellPrice}G (x{currentSlot.count})";
        }
    }

    /// <summary>
    /// Check if current item can be sold
    /// </summary>
    private bool CanSellCurrentItem()
    {
        if (currentSlot == null || currentSlot.type == CollectableType.NONE || currentSlot.count <= 0)
            return false;

        if (shopManager == null || shopManager.shopData == null)
            return false;

        ShopItem shopItem = shopManager.shopData.GetShopItem(currentSlot.type);
        return shopItem != null && shopItem.canSell;
    }

    /// <summary>
    /// Get sell price for current item
    /// </summary>
    private int GetSellPrice()
    {
        if (!CanSellCurrentItem()) return 0;

        ShopItem shopItem = shopManager.shopData.GetShopItem(currentSlot.type);
        return shopItem != null ? shopItem.sellPrice : 0;
    }

    /// <summary>
    /// Sell one item from this slot
    /// </summary>
    private void SellOneItem()
    {
        if (!CanSellCurrentItem()) return;

        bool success = shopManager.SellItem(slotIndex, 1);
        if (success)
        {
            currentSlot.count -= 1;

            if (currentSlot.count > 0)
                quantityText.text = currentSlot.count.ToString();
            else
                quantityText.text = "";

            UpdateSellDisplay(); // 🔄 Refresh UI hiển thị
            Debug.Log($"Slot_UI: Sold 1x {currentSlot.type} for {GetSellPrice()}G");
        }
    }

    /// <summary>
    /// Handle sell button click
    /// </summary>
    private void OnSellButtonClick()
    {
        SellOneItem();
    }

    /// <summary>
    /// Set sell overlay visibility
    /// </summary>
    private void SetSellOverlayVisible(bool visible)
    {
        if (sellOverlay != null)
            sellOverlay.SetActive(visible);
    }

    /// <summary>
    /// Set slot background color
    /// </summary>
    private void SetSlotBackgroundColor(Color color)
    {
        if (slotBackground != null)
            slotBackground.color = color;
    }

    /// <summary>
    /// Get current slot data
    /// </summary>
    public Inventory.Slot GetCurrentSlot()
    {
        return currentSlot;
    }
}
