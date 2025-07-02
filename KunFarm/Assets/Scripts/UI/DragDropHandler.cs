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
    public Inventory inventoryUI;

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
        Debug.Log($"inventoryUI: {inventoryUI != null}");
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
        if (currentSlot == null && currentTool == null) 
        {
            return;
        }
        
        if (currentSlot != null && currentSlot.type == CollectableType.NONE) 
        {
            return;
        }

        draggedItem = this;
        
        // Create drag object
        CreateDragObject();

        // Make original slot transparent but don't block raycasts completely
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
        if (draggedItem == null || draggedItem == this) 
        {
            return;
        }

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
            text.raycastTarget = false; // Ensure quantity text doesn't block raycasts
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
        if (draggedSlot == null) 
        {
            return;
        }

        ToolManager toolManager = FindObjectOfType<ToolManager>();
        
        if (inventoryUI == null || toolManager == null) 
        {
            return;
        }

        // Handle different drop scenarios
        if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Toolbar)
        {
            MoveInventoryToToolbar(draggedSlot, inventoryUI, toolManager, slotIndex);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Inventory)
        {
            MoveToolbarToInventory(draggedSlot, inventoryUI, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Toolbar)
        {
            SwapToolbarSlots(draggedSlot, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Inventory)
        {
            SwapInventorySlots(draggedSlot, inventoryUI);
        }
    }

    private void MoveInventoryToToolbar(
        DragDropHandler draggedSlot,
        Inventory inventoryUI,
        ToolManager toolManager,
        int toolbarSlotIndex)
    {
        // Null checks
        if (draggedSlot == null ||
            draggedSlot.currentSlot == null ||
            inventoryUI == null ||
            toolManager == null)
        {
            return;
        }

        var slotData = draggedSlot.currentSlot;
        
        // Check if slot is empty
        if (slotData.type == CollectableType.NONE || slotData.count <= 0)
        {
            return;
        }

        // Check if this item can become a tool
        bool canBeTool = ToolHelpers.CanBeTool(slotData.type);
        
        if (!canBeTool)
        {
            return;
        }

        int quantityToMove = slotData.count;
        CollectableType type = slotData.type;

        // Lấy tool hiện tại ở slot đích
        Tool existingTool = toolManager.GetToolAtIndex(toolbarSlotIndex);

        if (existingTool != null)
        {
            // Nếu cùng loại thì merge
            CollectableType existingType = ToolHelpers.GetCollectableFromTool(existingTool);
            
            if (existingType == type)
            {
                existingTool.quantity += quantityToMove;

                // Xoá toàn bộ số đó khỏi inventory
                inventoryUI.player.inventory.ClearSlot(draggedSlot.slotIndex);
                inventoryUI.Refresh();
                toolManager.UpdateToolbarDisplay();
                return;
            }

            // Nếu khác loại, trả tool cũ về inventory như trước
            CollectableType oldType = ToolHelpers.GetCollectableFromTool(existingTool);
            
            if (oldType != CollectableType.NONE)
            {
                int oldQty = existingTool.quantity > 0 ? existingTool.quantity : 1;
                inventoryUI.player.inventory.AddItemByType(oldType, existingTool.toolIcon, oldQty);
            }
        }

        // Tạo tool mới từ toàn bộ số lượng trong inventory
        Tool newTool = ToolHelpers.CreateToolFromCollectable(
            type,
            slotData.icon,
            quantityToMove);

        if (newTool != null)
        {
            // Đặt lên toolbar
            toolManager.SetToolAtIndex(toolbarSlotIndex, newTool);

            // Xoá slot inventory gốc
            inventoryUI.player.inventory.ClearSlot(draggedSlot.slotIndex);
            
            inventoryUI.Refresh();
            toolManager.UpdateToolbarDisplay();
        }
    }

    private void MoveToolbarToInventory(DragDropHandler draggedSlot, Inventory inventoryUI, ToolManager toolManager)
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

            // Add toàn bộ quantity từ tool vào inventory
            inventoryUI.player.inventory.AddItemByType(
                collectableType,
                draggedSlot.currentTool.toolIcon,
                toolQuantity  // Move toàn bộ quantity
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

    private void SwapInventorySlots(DragDropHandler draggedSlot, Inventory inventoryUI)
    {
        var inventory = inventoryUI.player.inventory;

        // Get actual inventory slots (not DragDropHandler references which can be null)
        var targetSlot = inventory.slots[slotIndex];
        var sourceSlot = inventory.slots[draggedSlot.slotIndex];

        // Create temp slot to store target data
        var tempSlot = new Inventory.Slot();
        tempSlot.type = targetSlot.type;
        tempSlot.count = targetSlot.count;
        tempSlot.icon = targetSlot.icon;
        tempSlot.maxAllowed = targetSlot.maxAllowed;

        // Copy source to target
        targetSlot.type = sourceSlot.type;
        targetSlot.count = sourceSlot.count;
        targetSlot.icon = sourceSlot.icon;
        targetSlot.maxAllowed = sourceSlot.maxAllowed;

        // Copy temp (original target) to source
        sourceSlot.type = tempSlot.type;
        sourceSlot.count = tempSlot.count;
        sourceSlot.icon = tempSlot.icon;
        sourceSlot.maxAllowed = tempSlot.maxAllowed;

        // Trigger inventory changed event
        inventory.NotifyInventoryChanged();
        inventoryUI.Refresh();
    }


}

public enum SlotType
{
    Inventory,
    Toolbar
}