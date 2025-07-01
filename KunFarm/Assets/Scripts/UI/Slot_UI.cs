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

    // / <summary>
    // / Get current slot data
    // / </summary>
    public Inventory.Slot GetCurrentSlot()
    {
        return currentSlot;
    }
}
