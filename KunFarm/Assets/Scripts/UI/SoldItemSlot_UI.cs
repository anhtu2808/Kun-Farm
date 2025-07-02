using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI component cho items đã đăng bán online
/// Hiển thị thông tin item + click để claim nếu đã bán
/// Phân biệt trạng thái bằng màu background
/// Có thể hiển thị slot rỗng
/// </summary>
public class SoldItemSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image backgroundImage; // Background để đổi màu

    [Header("Color Settings")]
    [SerializeField] private Color sellingColor = new Color(1f, 1f, 0.8f, 1f); // Màu vàng nhạt cho đang bán
    [SerializeField] private Color soldColor = new Color(0.8f, 1f, 0.8f, 1f); // Màu xanh nhạt cho đã bán
    [SerializeField] private Color emptyColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Màu xám nhạt cho slot rỗng

    private SellItemResponse itemData;
    private OnlSellShopManager shopManager;
    private Action<SellItemResponse> onClaimCallback;
    private Button slotButton; // Button cho toàn bộ slot
    private bool isEmpty = true; // Trạng thái slot rỗng

    private void Awake()
    {
        // Đảm bảo có Button component cho toàn bộ slot
        slotButton = GetComponent<Button>();
        if (slotButton == null)
        {
            slotButton = gameObject.AddComponent<Button>();
        }

        // Nếu chưa có background image, tạo mới
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
        }

        // Khởi tạo slot rỗng
        SetupEmptySlot();
    }

    /// <summary>
    /// Setup slot rỗng (không có data)
    /// </summary>
    public void SetupEmptySlot()
    {
        isEmpty = true;
        itemData = null;
        
        // Ẩn tất cả UI elements
        if (itemIcon != null) itemIcon.gameObject.SetActive(false);
        if (itemNameText != null) itemNameText.text = "";
        if (quantityText != null) quantityText.text = "";
        if (priceText != null) priceText.text = "";
        if (statusText != null) statusText.text = "Slot trống";
        
        // Set background color cho slot rỗng
        if (backgroundImage != null)
            backgroundImage.color = emptyColor;
        
        // Disable button cho slot rỗng
        if (slotButton != null)
        {
            slotButton.interactable = false;
            slotButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Setup UI slot với data từ server
    /// </summary>
    public void Setup(SellItemResponse data, OnlSellShopManager manager, Action<SellItemResponse> claimCallback)
    {
        isEmpty = false;
        itemData = data;
        shopManager = manager;
        onClaimCallback = claimCallback;

        UpdateUI();
        SetupSlotButton();
    }

    private void UpdateUI()
    {
        if (itemData == null) 
        {
            SetupEmptySlot();
            return;
        }

        // Set item name
        if (itemNameText != null)
            itemNameText.text = itemData.collectableType;

        // Set quantity  
        if (quantityText != null)
            quantityText.text = $"x{itemData.quantity}";

        // Set price
        if (priceText != null)
            priceText.text = $"{itemData.price}G";

        // Set icon từ ItemManager
        if (itemIcon != null)
        {
            LoadItemIcon();
        }

        // Set status và background color
        UpdateStatusAndVisual();
    }

    /// <summary>
    /// Load icon từ ItemManager dựa trên collectableType
    /// </summary>
    private void LoadItemIcon()
    {
        try
        {
            // Parse CollectableType từ string
            if (System.Enum.TryParse<CollectableType>(itemData.collectableType, ignoreCase: true, out var parsedType))
            {
                // Tìm ItemManager trong scene
                ItemManager itemManager = FindObjectOfType<ItemManager>();
                if (itemManager != null)
                {
                    // Lấy Collectable từ ItemManager
                    var collectable = itemManager.GetItemByType(parsedType);
                    if (collectable != null && collectable.icon != null)
                    {
                        itemIcon.sprite = collectable.icon;
                        itemIcon.gameObject.SetActive(true);
                        Debug.Log($"✅ [SoldItemSlot] Loaded icon for {itemData.collectableType}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ [SoldItemSlot] No icon found for {itemData.collectableType}");
                        itemIcon.gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError("❌ [SoldItemSlot] ItemManager not found in scene!");
                    itemIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError($"❌ [SoldItemSlot] Cannot parse CollectableType: {itemData.collectableType}");
                itemIcon.gameObject.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [SoldItemSlot] Error loading icon: {e.Message}");
            itemIcon.gameObject.SetActive(false);
        }
    }

    private void UpdateStatusAndVisual()
    {
        if (itemData.canBuy)
        {
            // Item vẫn đang bán - màu vàng nhạt
            if (statusText != null)
                statusText.text = "Đang bán";

            if (backgroundImage != null)
                backgroundImage.color = sellingColor;

            // Button vẫn clickable nhưng sẽ không làm gì
            if (slotButton != null)
                slotButton.interactable = true;
        }
        else
        {
            // Item đã được bán - màu xanh nhạt, có thể claim
            if (statusText != null)
                statusText.text = $"Đã bán - Click để claim {itemData.price}G";

            if (backgroundImage != null)
                backgroundImage.color = soldColor;

            if (slotButton != null)
                slotButton.interactable = true;
        }
    }

    private void SetupSlotButton()
    {
        if (slotButton != null)
        {
            // Clear existing listeners
            slotButton.onClick.RemoveAllListeners();
            
            // Add click event
            slotButton.onClick.AddListener(OnSlotClick);
        }
    }

    private void OnSlotClick()
    {
        if (isEmpty || itemData == null)
        {
            Debug.Log("[SoldItemSlot_UI] Slot rỗng - không làm gì");
            return;
        }

        if (itemData.canBuy)
        {
            // Item đang bán - không làm gì, chỉ log
            Debug.Log($"[SoldItemSlot_UI] Item {itemData.collectableType} is still for sale");
            return;
        }

        // Item đã bán - proceed với claim
        Debug.Log($"[SoldItemSlot_UI] Claiming item: {itemData.collectableType} for {itemData.price}G");
        
        // Disable button để tránh double-click
        if (slotButton != null)
            slotButton.interactable = false;

        // Call callback
        onClaimCallback?.Invoke(itemData);
    }

    /// <summary>
    /// Re-enable slot button (gọi khi claim thất bại)
    /// </summary>
    public void EnableSlotButton()
    {
        if (slotButton != null && !isEmpty)
            slotButton.interactable = true;
    }

    /// <summary>
    /// Get item ID for tracking
    /// </summary>
    public int GetItemId()
    {
        return itemData?.id ?? -1;
    }

    /// <summary>
    /// Destroy this slot with animation
    /// </summary>
    public void DestroySlot()
    {
        Debug.Log($"🗑️ [SoldItemSlot] Destroying slot for item ID: {GetItemId()}");
        
        // Optional: Thêm animation trước khi destroy
        // StartCoroutine(DestroyWithAnimation());
        
        // Immediate destroy cho đơn giản
        Destroy(gameObject);
    }

    /// <summary>
    /// Check if this slot is empty
    /// </summary>
    public bool IsEmpty => isEmpty;
}