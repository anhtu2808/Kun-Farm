using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Handles drag & drop functionality for inventory and toolbar slots
/// </summary>
public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    [Header("Drag Settings")]
    public Canvas canvas;

    private GameObject dragObject;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // Slot data
    private SlotType slotType;
    private int slotIndex;
    private Inventory.Slot currentSlot;
    private Tool currentTool;

    // Static reference to track what's being dragged
    private static DragDropHandler draggedItem;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(SlotType type, int index)
    {
        slotType = type;
        slotIndex = index;
    }

    public void SetSlotData(Inventory.Slot slot)
    {
        currentSlot = slot;
        currentTool = null;
    }

    public void SetToolData(Tool tool)
    {
        currentTool = tool;
        currentSlot = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only allow drag if there's something in this slot
        if (currentSlot == null && currentTool == null) return;
        if (currentSlot != null && currentSlot.type == CollectableType.NONE) return;

        draggedItem = this;

        // Create drag object
        CreateDragObject();

        // Make original slot transparent
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore original slot
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Destroy drag object
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }

        draggedItem = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedItem == null || draggedItem == this) return;

        // Handle the swap/move
        HandleDrop(draggedItem);
    }

    private void CreateDragObject()
    {
        dragObject = new GameObject("DragObject");
        dragObject.transform.SetParent(canvas.transform, false);

        // Add Image component
        Image dragImage = dragObject.AddComponent<Image>();

        if (currentSlot != null && currentSlot.icon != null)
        {
            dragImage.sprite = currentSlot.icon;
        }
        else if (currentTool != null && currentTool.toolIcon != null)
        {
            dragImage.sprite = currentTool.toolIcon;
        }

        dragImage.raycastTarget = false;

        // Add quantity text if needed
        if (currentSlot != null && currentSlot.count > 1)
        {
            GameObject textObj = new GameObject("Quantity");
            textObj.transform.SetParent(dragObject.transform, false);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = currentSlot.count.ToString();
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        // Set size
        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;

        // Set initial position
        dragObject.transform.position = transform.position;
    }

    private void HandleDrop(DragDropHandler draggedSlot)
    {
        // Null check for dragged slot
        if (draggedSlot == null) return;

        // Get references to managers
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        ToolManager toolManager = FindObjectOfType<ToolManager>();

        if (inventoryUI == null || toolManager == null) return;

        // Handle different drop scenarios
        if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Toolbar)
        {
            // Moving from inventory to toolbar
            MoveInventoryToToolbar(draggedSlot, inventoryUI, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Inventory)
        {
            // Moving from toolbar to inventory
            MoveToolbarToInventory(draggedSlot, inventoryUI, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Toolbar)
        {
            // Swapping toolbar slots
            SwapToolbarSlots(draggedSlot, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Inventory)
        {
            // Swapping inventory slots
            SwapInventorySlots(draggedSlot, inventoryUI);
        }
    }

    private void MoveInventoryToToolbar(DragDropHandler draggedSlot, InventoryUI inventoryUI, ToolManager toolManager)
    {
        // Null checks
        if (draggedSlot == null || draggedSlot.currentSlot == null || inventoryUI == null || toolManager == null) return;
        
        // Check if slot is empty
        if (draggedSlot.currentSlot.type == CollectableType.NONE || draggedSlot.currentSlot.count <= 0) return;
        
        // Check if this item can become a tool
        if (!ToolHelpers.CanBeTool(draggedSlot.currentSlot.type)) return;
        
        // Convert collectable to tool với quantity từ inventory
        Tool newTool = ToolHelpers.CreateToolFromCollectable(
            draggedSlot.currentSlot.type, 
            draggedSlot.currentSlot.icon, 
            draggedSlot.currentSlot.count
        );
        
        if (newTool != null)
        {
            // Additional null checks
            if (inventoryUI.player == null || inventoryUI.player.inventory == null) return;
            
            // Set tool in toolbar
            toolManager.SetToolAtIndex(slotIndex, newTool);
            
            // Remove item from inventory
            inventoryUI.player.inventory.Remove(draggedSlot.slotIndex);
            inventoryUI.Refresh();
            
            // Update toolbar display
            toolManager.UpdateToolbarDisplay();
        }
    }

    private void MoveToolbarToInventory(DragDropHandler draggedSlot, InventoryUI inventoryUI, ToolManager toolManager)
    {
        // Null checks
        if (draggedSlot == null || draggedSlot.currentTool == null || inventoryUI == null || toolManager == null) return;
        
        if (inventoryUI.player == null || inventoryUI.player.inventory == null) return;
        
        // Convert tool to collectable type
        CollectableType collectableType = ToolHelpers.GetCollectableFromTool(draggedSlot.currentTool);
        
        if (collectableType != CollectableType.NONE)
        {
            // Get quantity từ tool (Hand tool có quantity = -1 nên convert thành 1)
            int toolQuantity = draggedSlot.currentTool.quantity > 0 ? draggedSlot.currentTool.quantity : 1;
            
            // Add to inventory với quantity từ tool
            inventoryUI.player.inventory.AddItemByType(
                collectableType, 
                draggedSlot.currentTool.toolIcon, 
                toolQuantity
            );
            
            // Remove tool from toolbar
            toolManager.SetToolAtIndex(draggedSlot.slotIndex, null);
            
            // Update displays
            inventoryUI.Refresh();
            toolManager.UpdateToolbarDisplay();
        }
    }

    private void SwapToolbarSlots(DragDropHandler draggedSlot, ToolManager toolManager)
    {
        // Swap tools between toolbar slots
        Tool tempTool = currentTool;
        toolManager.SetToolAtIndex(slotIndex, draggedSlot.currentTool);
        toolManager.SetToolAtIndex(draggedSlot.slotIndex, tempTool);

        toolManager.UpdateToolbarDisplay();
    }

    private void SwapInventorySlots(DragDropHandler draggedSlot, InventoryUI inventoryUI)
    {
        // Swap inventory items
        var inventory = inventoryUI.player.inventory;
        var tempSlot = new Inventory.Slot();

        // Copy current slot to temp
        tempSlot.type = currentSlot.type;
        tempSlot.count = currentSlot.count;
        tempSlot.icon = currentSlot.icon;
        tempSlot.maxAllowed = currentSlot.maxAllowed;

        // Copy dragged slot to current
        inventory.slots[slotIndex].type = draggedSlot.currentSlot.type;
        inventory.slots[slotIndex].count = draggedSlot.currentSlot.count;
        inventory.slots[slotIndex].icon = draggedSlot.currentSlot.icon;
        inventory.slots[slotIndex].maxAllowed = draggedSlot.currentSlot.maxAllowed;

        // Copy temp to dragged slot
        inventory.slots[draggedSlot.slotIndex].type = tempSlot.type;
        inventory.slots[draggedSlot.slotIndex].count = tempSlot.count;
        inventory.slots[draggedSlot.slotIndex].icon = tempSlot.icon;
        inventory.slots[draggedSlot.slotIndex].maxAllowed = tempSlot.maxAllowed;

        inventoryUI.Refresh();
    }


}

public enum SlotType
{
    Inventory,
    Toolbar
}