using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot_UI : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    [SerializeField] private GameObject highlight;
    
    private DragDropHandler dragDropHandler;
    private System.Action<int> onSlotClicked;
    private int slotIndex;
    
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

    public void SetItem(Inventory.Slot slot)
    {
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
        // Only handle left click and not during drag operations
        if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
        {
            onSlotClicked?.Invoke(slotIndex);
        }
    }
}
