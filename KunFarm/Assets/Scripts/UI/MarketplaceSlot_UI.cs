using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component cho một slot trong marketplace
/// </summary>
public class MarketplaceSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI sellerNameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI expiryText;
    public GameObject buyButtonObject;
    public GameObject expiredOverlay;

    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color expiredColor = Color.gray;
    public Color expensiveColor = Color.red;

    // Data
    private MarketplaceItem currentItem;
    private MarketplaceManager marketplaceManager;

    // Events
    public System.Action<MarketplaceSlot_UI> OnBuyClicked;

    private void Awake()
    {
        if (buyButtonObject != null)
        {
            var button = buyButtonObject.GetComponent<Button>();
            if (button == null)
                button = buyButtonObject.AddComponent<Button>();

            button.onClick.AddListener(() => OnBuyClicked?.Invoke(this));
        }
    }

    /// <summary>
    /// Initialize marketplace slot với data
    /// </summary>
    public void Initialize(MarketplaceItem item, MarketplaceManager manager)
    {
        currentItem = item;
        marketplaceManager = manager;
        UpdateDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị của slot
    /// </summary>
    public void UpdateDisplay()
    {
        if (currentItem == null) return;

        // Cập nhật icon và tên
        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.itemIcon;
            itemIcon.color = currentItem.IsActive() ? normalColor : expiredColor;
        }

        if (itemNameText != null)
            itemNameText.text = currentItem.itemName;

        if (sellerNameText != null)
            sellerNameText.text = $"Bởi: {currentItem.sellerName}";

        // Cập nhật giá
        if (priceText != null)
        {
            priceText.text = $"{currentItem.price}G";
            
            // Đổi màu nếu giá quá cao
            if (currentItem.price > 1000) // Có thể điều chỉnh ngưỡng
                priceText.color = expensiveColor;
            else
                priceText.color = Color.white;
        }

        // Cập nhật số lượng
        if (quantityText != null)
        {
            quantityText.text = $"Số lượng: {currentItem.quantity}";
        }

        // Cập nhật thời gian hết hạn
        if (expiryText != null)
        {
            var timeLeft = currentItem.expiryDate - System.DateTime.Now;
            if (timeLeft.TotalDays > 1)
            {
                expiryText.text = $"Còn {timeLeft.Days} ngày";
                expiryText.color = Color.green;
            }
            else if (timeLeft.TotalHours > 1)
            {
                expiryText.text = $"Còn {timeLeft.Hours} giờ";
                expiryText.color = Color.yellow;
            }
            else
            {
                expiryText.text = "Sắp hết hạn";
                expiryText.color = Color.red;
            }
        }

        // Cập nhật overlay hết hạn
        if (expiredOverlay != null)
            expiredOverlay.SetActive(!currentItem.IsActive());

        // Cập nhật buy button
        UpdateBuyButton();
    }

    /// <summary>
    /// Cập nhật trạng thái buy button
    /// </summary>
    private void UpdateBuyButton()
    {
        if (buyButtonObject == null) return;

        var button = buyButtonObject.GetComponent<Button>();
        if (button != null)
        {
            bool canBuy = currentItem.IsActive() && 
                         marketplaceManager != null && 
                         marketplaceManager.player != null &&
                         marketplaceManager.player.wallet.Money >= currentItem.price;

            button.interactable = canBuy;
            button.gameObject.SetActive(currentItem.IsActive());
        }
    }

    /// <summary>
    /// Thực hiện mua item
    /// </summary>
    public async void BuyItem(int quantity = 1)
    {
        if (marketplaceManager == null || currentItem == null) return;

        if (await marketplaceManager.BuyItem(currentItem.id, quantity))
        {
            UpdateDisplay();
            Debug.Log($"Đã mua {quantity} {currentItem.itemName} từ {currentItem.sellerName}");
        }
    }

    /// <summary>
    /// Getter cho MarketplaceItem hiện tại
    /// </summary>
    public MarketplaceItem GetMarketplaceItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Refresh display khi có thay đổi
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
} 