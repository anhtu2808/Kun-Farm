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
        Debug.Log($"[OnBeginDrag] Starting drag from slot {slotIndex} of type {slotType}");
        
        // Only allow drag if there's something in this slot
        if (currentSlot == null && currentTool == null) 
        {
            Debug.LogWarning($"[OnBeginDrag] No item to drag - currentSlot and currentTool are both null");
            return;
        }
        
        if (currentSlot != null && currentSlot.type == CollectableType.NONE) 
        {
            Debug.LogWarning($"[OnBeginDrag] Current slot has no item (type = NONE)");
            return;
        }

        draggedItem = this;
        
        if (currentSlot != null)
        {
            Debug.Log($"[OnBeginDrag] ✅ Dragging item: {currentSlot.type} (quantity: {currentSlot.count}) from slot {slotIndex} of type {slotType}");
        }
        else if (currentTool != null)
        {
            Debug.Log($"[OnBeginDrag] ✅ Dragging tool: {currentTool.toolName} (quantity: {currentTool.quantity}) from slot {slotIndex} of type {slotType}");
        }
        
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
        Debug.Log($"[OnDrop] Drop detected on slot {slotIndex} of type {slotType}");
        
        if (draggedItem == null || draggedItem == this) 
        {
            Debug.LogWarning($"[OnDrop] Invalid drop: draggedItem={draggedItem != null}, same slot={draggedItem == this}");
            return;
        }

        Debug.Log($"[OnDrop] Valid drop from slot {draggedItem.slotIndex} ({draggedItem.slotType}) to slot {slotIndex} ({slotType})");
        
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
        Debug.Log($"[HandleDrop] Processing drop from {draggedSlot.slotType} slot {draggedSlot.slotIndex} to {slotType} slot {slotIndex}");
        
        // Null check for dragged slot
        if (draggedSlot == null) 
        {
            Debug.LogError("[HandleDrop] draggedSlot is null!");
            return;
        }

        ToolManager toolManager = FindObjectOfType<ToolManager>();
        Debug.Log($"[HandleDrop] inventoryUI: {inventoryUI != null}, toolManager: {toolManager != null}");
        
        if (inventoryUI == null || toolManager == null) 
        {
            Debug.LogError($"[HandleDrop] Missing components: inventoryUI={inventoryUI != null}, toolManager={toolManager != null}");
            return;
        }

        // Handle different drop scenarios
        if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Toolbar)
        {
            Debug.Log("[HandleDrop] 📦➡️🔧 Moving from inventory to toolbar");
            MoveInventoryToToolbar(draggedSlot, inventoryUI, toolManager, slotIndex);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Inventory)
        {
            Debug.Log("[HandleDrop] 🔧➡️📦 Moving from toolbar to inventory");
            MoveToolbarToInventory(draggedSlot, inventoryUI, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Toolbar && slotType == SlotType.Toolbar)
        {
            Debug.Log("[HandleDrop] 🔧↔️🔧 Swapping toolbar slots");
            SwapToolbarSlots(draggedSlot, toolManager);
        }
        else if (draggedSlot.slotType == SlotType.Inventory && slotType == SlotType.Inventory)
        {
            Debug.Log("[HandleDrop] 📦↔️📦 Swapping inventory slots");
            SwapInventorySlots(draggedSlot, inventoryUI);
        }
        else
        {
            Debug.LogWarning($"[HandleDrop] Unknown drop scenario: {draggedSlot.slotType} to {slotType}");
        }
    }

    private void MoveInventoryToToolbar(
        DragDropHandler draggedSlot,
        Inventory inventoryUI,
        ToolManager toolManager,
        int toolbarSlotIndex)
    {
        Debug.Log($"[MoveInventoryToToolbar] Starting move to slot {toolbarSlotIndex}");
        
        // Null checks
        if (draggedSlot == null ||
            draggedSlot.currentSlot == null ||
            inventoryUI == null ||
            toolManager == null)
        {
            Debug.LogError($"[MoveInventoryToToolbar] Null check failed: draggedSlot={draggedSlot != null}, currentSlot={draggedSlot?.currentSlot != null}, inventoryUI={inventoryUI != null}, toolManager={toolManager != null}");
            return;
        }

        var slotData = draggedSlot.currentSlot;
        Debug.Log($"[MoveInventoryToToolbar] Slot data: type={slotData.type}, count={slotData.count}");
        
        // Check if slot is empty
        if (slotData.type == CollectableType.NONE || slotData.count <= 0)
        {
            Debug.LogWarning($"[MoveInventoryToToolbar] Slot is empty or has no items: type={slotData.type}, count={slotData.count}");
            return;
        }

        // Check if this item can become a tool
        bool canBeTool = ToolHelpers.CanBeTool(slotData.type);
        Debug.Log($"[MoveInventoryToToolbar] Can be tool check: {slotData.type} -> {canBeTool}");
        
        if (!canBeTool)
        {
            Debug.LogWarning($"[MoveInventoryToToolbar] Item {slotData.type} cannot become a tool");
            return;
        }

        int quantityToMove = slotData.count;
        CollectableType type = slotData.type;
        Debug.Log($"[MoveInventoryToToolbar] Moving {quantityToMove} of {type} to toolbar slot {toolbarSlotIndex}");

        // Lấy tool hiện tại ở slot đích
        Tool existingTool = toolManager.GetToolAtIndex(toolbarSlotIndex);
        Debug.Log($"[MoveInventoryToToolbar] Existing tool at slot {toolbarSlotIndex}: {existingTool?.toolName ?? "None"}");

        if (existingTool != null)
        {
            // Nếu cùng loại thì merge
            CollectableType existingType = ToolHelpers.GetCollectableFromTool(existingTool);
            Debug.Log($"[MoveInventoryToToolbar] Existing tool type: {existingType}, comparing with: {type}");
            
            if (existingType == type)
            {
                existingTool.quantity += quantityToMove;
                Debug.Log($"[MoveInventoryToToolbar] ✅ Merged {quantityToMove} of {type} into toolbar slot {toolbarSlotIndex}. New qty = {existingTool.quantity}");

                // Xoá toàn bộ số đó khỏi inventory
                inventoryUI.player.inventory.ClearSlot(draggedSlot.slotIndex);
                inventoryUI.Refresh();
                toolManager.UpdateToolbarDisplay();
                return;
            }

            // Nếu khác loại, trả tool cũ về inventory như trước
            CollectableType oldType = ToolHelpers.GetCollectableFromTool(existingTool);
            Debug.Log($"[MoveInventoryToToolbar] Different tool type. Moving {oldType} back to inventory");
            
            if (oldType != CollectableType.NONE)
            {
                int oldQty = existingTool.quantity > 0 ? existingTool.quantity : 1;
                inventoryUI.player.inventory.AddItemByType(oldType, existingTool.toolIcon, oldQty);
                Debug.Log($"[MoveInventoryToToolbar] Moved {oldQty} of {oldType} back to inventory");
            }
        }

        // Tạo tool mới từ toàn bộ số lượng trong inventory
        Debug.Log($"[MoveInventoryToToolbar] Creating new tool from {type} with quantity {quantityToMove}");
        Tool newTool = ToolHelpers.CreateToolFromCollectable(
            type,
            slotData.icon,
            quantityToMove);

        if (newTool != null)
        {
            Debug.Log($"[MoveInventoryToToolbar] ✅ Successfully created {newTool.toolName} tool with quantity {newTool.quantity}");
            
            // Đặt lên toolbar
            toolManager.SetToolAtIndex(toolbarSlotIndex, newTool);
            Debug.Log($"[MoveInventoryToToolbar] Set tool at toolbar slot {toolbarSlotIndex}");

            // Xoá slot inventory gốc
            inventoryUI.player.inventory.ClearSlot(draggedSlot.slotIndex);
            Debug.Log($"[MoveInventoryToToolbar] Cleared inventory slot {draggedSlot.slotIndex}");
            
            inventoryUI.Refresh();
            toolManager.UpdateToolbarDisplay();
            Debug.Log($"[MoveInventoryToToolbar] ✅ Move completed successfully!");
        }
        else
        {
            Debug.LogError($"[MoveInventoryToToolbar] ❌ Failed to create tool from {type}");
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