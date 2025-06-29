using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject chứa tất cả items có thể mua/bán trong shop
/// </summary>
[CreateAssetMenu(fileName = "ShopData", menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    [Header("Shop Items")]
    public List<ShopItem> shopItems = new List<ShopItem>();

    /// <summary>
    /// Lấy ShopItem theo CollectableType
    /// </summary>
    public ShopItem GetShopItem(CollectableType type)
    {
        return shopItems.Find(item => item.collectableType == type);
    }

    /// <summary>
    /// Kiểm tra xem item có thể mua không
    /// </summary>
    public bool CanBuyItem(CollectableType type)
    {
        ShopItem item = GetShopItem(type);
        return item != null && item.canBuy && item.buyPrice > 0;
    }

    /// <summary>
    /// Kiểm tra xem item có thể bán không
    /// </summary>
    public bool CanSellItem(CollectableType type)
    {
        ShopItem item = GetShopItem(type);
        return item != null && item.canSell && item.sellPrice > 0;
    }

    /// <summary>
    /// Lấy giá mua của item
    /// </summary>
    public int GetBuyPrice(CollectableType type)
    {
        ShopItem item = GetShopItem(type);
        return item != null ? item.buyPrice : 0;
    }

    /// <summary>
    /// Lấy giá bán của item
    /// </summary>
    public int GetSellPrice(CollectableType type)
    {
        ShopItem item = GetShopItem(type);
        return item != null ? item.sellPrice : 0;
    }
}

/// <summary>
/// Dữ liệu của một item trong shop
/// </summary>
[System.Serializable]
public class ShopItem
{
    [Header("Item Info")]
    public CollectableType collectableType;
    public string itemName;
    public Sprite itemIcon;
    
    [Header("Buy Settings")]
    public bool canBuy = true;
    public int buyPrice = 10;
    
    [Header("Sell Settings")]
    public bool canSell = true;
    public int sellPrice = 5;
    
    [Header("Shop Display")]
    [Tooltip("Có hiển thị trong shop không (để mua)")]
    public bool showInShop = true;
    
    [Header("Stock Settings (Optional)")]
    [Tooltip("Giới hạn số lượng trong shop (-1 = vô hạn)")]
    public int stockLimit = -1;
    public int currentStock = -1;

    /// <summary>
    /// Kiểm tra xem còn hàng trong shop không
    /// </summary>
    public bool HasStock()
    {
        return stockLimit < 0 || currentStock > 0;
    }

    /// <summary>
    /// Giảm stock khi mua
    /// </summary>
    public void ConsumeStock(int amount = 1)
    {
        if (stockLimit >= 0)
        {
            currentStock = Mathf.Max(0, currentStock - amount);
        }
    }

    /// <summary>
    /// Khôi phục stock (có thể dùng để refresh shop hàng ngày)
    /// </summary>
    public void RestoreStock()
    {
        if (stockLimit >= 0)
        {
            currentStock = stockLimit;
        }
    }
} 