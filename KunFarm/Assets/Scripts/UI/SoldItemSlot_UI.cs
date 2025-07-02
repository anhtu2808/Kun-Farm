using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI component cho items đã đăng bán online
/// Hiển thị thông tin item + button claim nếu đã bán
/// </summary>
public class SoldItemSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button claimButton;

    private SellItemResponse itemData;
    private OnlSellShopManager shopManager;
    private Action<SellItemResponse> onClaimCallback;

    /// <summary>
    /// Setup UI slot với data từ server
    /// </summary>
    public void Setup(SellItemResponse data, OnlSellShopManager manager, Action<SellItemResponse> claimCallback)
    {
        itemData = data;
        shopManager = manager;
        onClaimCallback = claimCallback;

        UpdateUI();
        SetupClaimButton();
    }

    private void UpdateUI()
    {
        if (itemData == null) return;

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

        // Set status và button state
        UpdateStatusAndButton();
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

    private void UpdateStatusAndButton()
    {
        if (itemData.canBuy)
        {
            // Item vẫn đang bán
            if (statusText != null)
                statusText.text = "Đang bán";

            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // Item đã được bán, có thể claim
            if (statusText != null)
                statusText.text = "Đã bán";

            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(true);
                var buttonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = $"Claim {itemData.price}G";
            }
        }
    }

    private void SetupClaimButton()
    {
        if (claimButton != null)
        {
            // Clear existing listeners
            claimButton.onClick.RemoveAllListeners();
            
            // Add click event
            claimButton.onClick.AddListener(OnClaimButtonClick);
        }
    }

    private void OnClaimButtonClick()
    {
        if (itemData == null || itemData.canBuy)
        {
            Debug.LogWarning($"[SoldItemSlot_UI] Cannot claim item {itemData?.collectableType} - still for sale");
            return;
        }

        Debug.Log($"[SoldItemSlot_UI] Claiming item: {itemData.collectableType} for {itemData.price}G");
        
        // Disable button để tránh double-click
        if (claimButton != null)
            claimButton.interactable = false;

        // Call callback
        onClaimCallback?.Invoke(itemData);
    }

    /// <summary>
    /// Re-enable claim button (gọi khi claim thất bại)
    /// </summary>
    public void EnableClaimButton()
    {
        if (claimButton != null)
            claimButton.interactable = true;
    }
}