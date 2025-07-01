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
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    private ShopSlotData slotData;
    private ShopManager shopManager;
    private Player player;

    public void Setup(ShopSlotData data, ShopManager manager)
    {
        Debug.Log($"Setting up shop slot for: {data.collectableType} - price: {data.buyPrice} - {data.currentStock}/{data.stockLimit}");

        slotData = data;
        shopManager = manager;

        priceText.text = $"{data.buyPrice}G";
        // stockText.text = $"{data.currentStock}/{data.stockLimit}";

        Sprite icon = Resources.Load<Sprite>($"Sprites/{data.icon}");
        itemIcon.sprite = icon;
    }

    public void OnBuyClicked()
    {
        if (slotData != null && shopManager != null)
        {
            if (slotData.currentStock >= slotData.stockLimit)
            {
                Debug.LogWarning("Item hết hàng!");
                return;
            }

            Debug.Log($"Đã click mua: {slotData.itemName}");
            shopManager.BuyItem(slotData);
        }
    }

    /// <summary>
    /// Initialize shop slot với data
    /// </summary>
    // public void Initialize(ShopItem shopItem, ShopManager manager, Player playerRef)
    // {
    //     currentShopItem = shopItem;
    //     shopManager = manager;
    //     player = playerRef;

    //     UpdateDisplay();
    // }

    /// <summary>
    /// Cập nhật hiển thị của slot
    /// </summary>
    // public void UpdateDisplay()
    // {
    //     if (currentShopItem == null) return;

    //     // Cập nhật icon và tên
    //     if (itemIcon != null)
    //     {
    //         itemIcon.sprite = currentShopItem.itemIcon;
    //         itemIcon.color = currentShopItem.HasStock() ? normalColor : outOfStockColor;
    //     }

    //     if (itemNameText != null)
    //         itemNameText.text = currentShopItem.itemName;

    //     // ✅ CHỈ HIỂN THỊ GIÁ MUA - SHOP CHỈ DÀNH CHO MUA
    //     if (buyPriceText != null)
    //     {
    //         if (currentShopItem.canBuy && currentShopItem.showInShop)
    //         {
    //             buyPriceText.text = $"Mua: {currentShopItem.buyPrice}G";
    //             buyPriceText.gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             buyPriceText.gameObject.SetActive(false);
    //         }
    //     }

    //     // Cập nhật stock
    //     if (stockText != null)
    //     {
    //         if (currentShopItem.stockLimit >= 0)
    //         {
    //             stockText.text = $"Stock: {currentShopItem.currentStock}";
    //             stockText.gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             stockText.gameObject.SetActive(false);
    //         }
    //     }

    //     // Cập nhật overlay hết hàng
    //     if (outOfStockOverlay != null)
    //         outOfStockOverlay.SetActive(!currentShopItem.HasStock());

    //     // Cập nhật buttons
    //     UpdateButtons();
    // }

    /// <summary>
    /// Cập nhật trạng thái buttons - CHỈ CHO MUA
    /// </summary>
    // private void UpdateButtons()
    // {
    //     if (player == null || currentShopItem == null) return;

    //     // ✅ CHỈ CẬP NHẬT BUY BUTTON
    //     if (buyButtonObject != null)
    //     {
    //         var button = buyButtonObject.GetComponent<Button>();
    //         if (button != null)
    //         {
    //             bool canBuy = currentShopItem.canBuy &&
    //                           currentShopItem.showInShop &&
    //                           currentShopItem.HasStock() &&
    //                           player.wallet.Money >= currentShopItem.buyPrice;

    //             button.interactable = canBuy;
    //             button.gameObject.SetActive(currentShopItem.canBuy && currentShopItem.showInShop);
    //         }
    //     }
    // }

    /// <summary>
    /// Lấy số lượng item trong inventory của player
    /// </summary>
    // private int GetPlayerItemCount()
    // {
    //     if (player == null || player.inventory == null || currentShopItem == null)
    //         return 0;

    //     int count = 0;
    //     foreach (var slot in player.inventory.slots)
    //     {
    //         if (slot.type == currentShopItem.collectableType)
    //             count += slot.count;
    //     }
    //     return count;
    // }

    /// <summary>
    /// Thực hiện mua item
    /// </summary>
    // public void BuyItem(int quantity = 1)
    // {
    //     if (shopManager == null || currentShopItem == null) return;

    //     if (shopManager.BuyItem(currentShopItem.collectableType, quantity))
    //     {
    //         UpdateDisplay();
    //     }
    // }

    /// <summary>
    /// Getter cho ShopItem hiện tại
    /// </summary>
    // public ShopItem GetShopItem()
    // {
    //     return currentShopItem;
    // }

    /// <summary>
    /// Refresh display khi có thay đổi
    /// </summary>
    // public void Refresh()
    // {
    //     UpdateDisplay();
    // }
}