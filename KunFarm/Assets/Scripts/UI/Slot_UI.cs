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
    public GameObject emptyOverlay;
    private DragDropHandler dragDropHandler;
    private System.Action<int> onSlotClicked;
    private int slotIndex;
    private Inventory.Slot currentSlot;

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
    }

    public void Setup(InventorySlotData data)
    {
        Debug.Log($"Setting up slot {data.slotIndex} with data: {data.icon}, quantity: {data.quantity}");
        if (data.collectableType == "NONE" || data.quantity <= 0)
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0); // ẩn icon

            quantityText.text = "";
            if (emptyOverlay != null)
                emptyOverlay.SetActive(true);
        }
        else
        {
            itemIcon.sprite = Resources.Load<Sprite>($"Sprites/{data.icon}");
            quantityText.text = data.quantity.ToString();
            if (emptyOverlay != null)
                emptyOverlay.SetActive(false);
        }

        if (dragDropHandler != null)
        {
            dragDropHandler.inventoryUI = FindObjectOfType<Inventory>();
        }
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
        if (dragDropHandler == null)
        {
            dragDropHandler = GetComponent<DragDropHandler>(); // Ép gán lại lần nữa
            if (dragDropHandler == null)
            {
                Debug.LogError($"[InitializeDragDrop] dragDropHandler is STILL null in slot {index}!");
                return;
            }
        }

        dragDropHandler.Initialize(slotType, index);
        dragDropHandler.inventoryUI = FindObjectOfType<Inventory>();
        Debug.Log($"[InitializeDragDrop] inventoryUI assigned: {dragDropHandler.inventoryUI != null}");

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

    /// <summary>
    /// Handle mouse clicks on the slot
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click to drop item
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            DropItem();
        }
        // Left-click for selection (existing behavior) 
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            onSlotClicked?.Invoke(slotIndex);
        }
    }

    /// <summary>
    /// Drop item from this slot to the ground
    /// </summary>
    private void DropItem()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        Player player = FindObjectOfType<Player>();
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        
        if (inventory == null || player == null || itemManager == null) return;
        
        // Check if this is an inventory slot and has items
        if (currentSlot != null && currentSlot.type != CollectableType.NONE && currentSlot.count > 0)
        {
            // Get the collectable prefab for this item type
            Collectable collectablePrefab = itemManager.GetItemByType(currentSlot.type);
            
            if (collectablePrefab != null)
            {
                // Drop the item
                player.DropItem(collectablePrefab);
                
                // Remove 1 item from inventory slot
                currentSlot.RemoveItem();
                
                // Show notification
                string itemName = currentSlot.type.ToString();
                SimpleNotificationPopup.Show($"Dropped {itemName}!");
                
                // If slot is empty, clear the slot data
                if (currentSlot.count <= 0)
                {
                    currentSlot.type = CollectableType.NONE;
                    currentSlot.icon = null;
                    SetEmpty();
                }
                else
                {
                    // Update display with new quantity
                    quantityText.text = currentSlot.count.ToString();
                }
                
                // Notify inventory of changes
                inventory.MarkInventoryChanged();
                inventory.NotifyInventoryChanged();
                
                Debug.Log($"[Slot_UI] Dropped {collectablePrefab.type} from slot {slotIndex}, remaining: {currentSlot.count}");
            }
            else
            {
                Debug.LogWarning($"[Slot_UI] No collectable prefab found for {currentSlot.type}");
                SimpleNotificationPopup.Show($"Cannot drop {currentSlot.type} - no prefab found!");
            }
        }
        else
        {
            SimpleNotificationPopup.Show("No item to drop! Right-click on items to drop them.");
        }
    }
}
