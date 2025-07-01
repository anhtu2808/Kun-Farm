using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot_UI : MonoBehaviour
{
    [Header("Basic UI")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    [SerializeField] private GameObject highlight;

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
            // UpdateSellDisplay();
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

    /// <summary>
    /// Get current slot data
    /// </summary>
    public Inventory.Slot GetCurrentSlot()
    {
        return currentSlot;
    }
}
