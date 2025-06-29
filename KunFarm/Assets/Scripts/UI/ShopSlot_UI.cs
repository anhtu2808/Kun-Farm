using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component cho một slot trong shop
/// </summary>
public class ShopSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI buyPriceText;
    public TextMeshProUGUI stockText;
    public Button buyButton;
    
    // ✅ SHOP CHỈ DÀNH CHO MUA - XÓA SELL FUNCTIONALITY
    [Header("Disabled Sell Features")]
    [System.Obsolete("Shop slots chỉ dành cho mua - không cần sell")]
    public TextMeshProUGUI sellPriceText;
    [System.Obsolete("Shop slots chỉ dành cho mua - không cần sell")]
    public Button sellButton;
    [System.Obsolete("Shop slots chỉ dành cho mua - không cần sell")]
    public Button sellAllButton;
    
    [Header("Visual States")]
    public GameObject outOfStockOverlay;
    public Color normalColor = Color.white;
    public Color outOfStockColor = Color.gray;
    
    // Data
    private ShopItem currentShopItem;
    private ShopManager shopManager;
    private Player player;
    
    // Events
    public System.Action<ShopSlot_UI> OnBuyClicked;
    
    // ✅ SHOP CHỈ DÀNH CHO MUA - XÓA SELL EVENTS
    [System.Obsolete("Shop slots chỉ dành cho mua - không cần sell events")]
    public System.Action<ShopSlot_UI> OnSellClicked;
    [System.Obsolete("Shop slots chỉ dành cho mua - không cần sell events")]
    public System.Action<ShopSlot_UI> OnSellAllClicked;

    private void Awake()
    {
        // ✅ CHỈ SETUP BUY BUTTON CHO SHOP
        if (buyButton != null)
            buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(this));
            
        // ✅ DISABLE SELL BUTTONS VÌ SHOP CHỈ DÀNH CHO MUA
        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(false);
            Debug.Log("ShopSlot_UI: Disabled sell button - Shop chỉ dành cho mua");
        }
            
        if (sellAllButton != null)
        {
            sellAllButton.gameObject.SetActive(false);
            Debug.Log("ShopSlot_UI: Disabled sellAll button - Shop chỉ dành cho mua");
        }
    }

    /// <summary>
    /// Initialize shop slot với data
    /// </summary>
    public void Initialize(ShopItem shopItem, ShopManager manager, Player playerRef)
    {
        currentShopItem = shopItem;
        shopManager = manager;
        player = playerRef;
        
        UpdateDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị của slot
    /// </summary>
    public void UpdateDisplay()
    {
        if (currentShopItem == null) return;

        // Cập nhật icon và tên
        if (itemIcon != null)
        {
            itemIcon.sprite = currentShopItem.itemIcon;
            itemIcon.color = currentShopItem.HasStock() ? normalColor : outOfStockColor;
        }
        
        if (itemNameText != null)
            itemNameText.text = currentShopItem.itemName;

        // ✅ CHỈ HIỂN THỊ GIÁ MUA - SHOP CHỈ DÀNH CHO MUA
        if (buyPriceText != null)
        {
            if (currentShopItem.canBuy && currentShopItem.showInShop)
            {
                buyPriceText.text = $"Mua: {currentShopItem.buyPrice}G";
                buyPriceText.gameObject.SetActive(true);
            }
            else
            {
                buyPriceText.gameObject.SetActive(false);
            }
        }

        // ✅ ẨN SELL PRICE VÌ SHOP CHỈ DÀNH CHO MUA
        if (sellPriceText != null)
        {
            sellPriceText.gameObject.SetActive(false);
        }

        // Cập nhật stock
        if (stockText != null)
        {
            if (currentShopItem.stockLimit >= 0)
            {
                stockText.text = $"Stock: {currentShopItem.currentStock}";
                stockText.gameObject.SetActive(true);
            }
            else
            {
                stockText.gameObject.SetActive(false);
            }
        }

        // Cập nhật overlay hết hàng
        if (outOfStockOverlay != null)
            outOfStockOverlay.SetActive(!currentShopItem.HasStock());

        // Cập nhật buttons
        UpdateButtons();
    }

    /// <summary>
    /// Cập nhật trạng thái buttons - CHỈ CHO MUA
    /// </summary>
    private void UpdateButtons()
    {
        if (player == null || currentShopItem == null) return;

        // ✅ CHỈ CẬP NHẬT BUY BUTTON
        if (buyButton != null)
        {
            bool canBuy = currentShopItem.canBuy && 
                         currentShopItem.showInShop && 
                         currentShopItem.HasStock() &&
                         player.wallet.Money >= currentShopItem.buyPrice;
            
            buyButton.interactable = canBuy;
            buyButton.gameObject.SetActive(currentShopItem.canBuy && currentShopItem.showInShop);
        }

        // ✅ ĐẢM BẢO SELL BUTTONS LUÔN DISABLED
        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(false);
        }

        if (sellAllButton != null)
        {
            sellAllButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Lấy số lượng item trong inventory của player
    /// </summary>
    private int GetPlayerItemCount()
    {
        if (player == null || player.inventory == null || currentShopItem == null)
            return 0;

        int count = 0;
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == currentShopItem.collectableType)
                count += slot.count;
        }
        return count;
    }

    /// <summary>
    /// Thực hiện mua item
    /// </summary>
    public void BuyItem(int quantity = 1)
    {
        if (shopManager == null || currentShopItem == null) return;

        if (shopManager.BuyItem(currentShopItem.collectableType, quantity))
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Thực hiện bán item (bán 1 item đầu tiên tìm thấy)
    /// </summary>
    public void SellItem(int quantity = 1)
    {
        if (shopManager == null || currentShopItem == null || player == null) return;

        // Tìm slot đầu tiên có item này
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == currentShopItem.collectableType && slot.count > 0)
            {
                int sellQuantity = Mathf.Min(quantity, slot.count);
                if (shopManager.SellItem(i, sellQuantity))
                {
                    UpdateDisplay();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Bán tất cả items cùng loại
    /// </summary>
    public void SellAllItems()
    {
        if (shopManager == null || currentShopItem == null) return;

        if (shopManager.SellAllItems(currentShopItem.collectableType))
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Getter cho ShopItem hiện tại
    /// </summary>
    public ShopItem GetShopItem()
    {
        return currentShopItem;
    }

    /// <summary>
    /// Refresh display khi có thay đổi
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
} 