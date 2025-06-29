using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject chứa cấu hình cho marketplace online
/// </summary>
[CreateAssetMenu(fileName = "MarketplaceData", menuName = "Marketplace/Marketplace Data")]
public class MarketplaceData : ScriptableObject
{
    [Header("Marketplace Configuration")]
    [Tooltip("Thời gian hết hạn mặc định cho item (ngày)")]
    public int defaultExpiryDays = 7;
    
    [Tooltip("Phí giao dịch (phần trăm)")]
    [Range(0f, 10f)]
    public float transactionFeePercent = 2f;
    
    [Tooltip("Số lượng item tối đa mỗi player có thể đăng bán")]
    public int maxItemsPerPlayer = 20;
    
    [Header("Item Categories")]
    public List<MarketplaceCategory> categories = new List<MarketplaceCategory>();
    
    /// <summary>
    /// Lấy category theo tên
    /// </summary>
    public MarketplaceCategory GetCategory(string categoryName)
    {
        return categories.Find(cat => cat.categoryName == categoryName);
    }
    
    /// <summary>
    /// Kiểm tra item có thể đăng bán không
    /// </summary>
    public bool CanListItem(CollectableType type)
    {
        // Kiểm tra xem item có trong danh sách được phép đăng bán không
        foreach (var category in categories)
        {
            if (category.allowedItems.Contains(type))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Lấy category của item
    /// </summary>
    public string GetItemCategory(CollectableType type)
    {
        foreach (var category in categories)
        {
            if (category.allowedItems.Contains(type))
                return category.categoryName;
        }
        return "Other";
    }
}

/// <summary>
/// Category cho marketplace items
/// </summary>
[System.Serializable]
public class MarketplaceCategory
{
    [Header("Category Info")]
    public string categoryName;
    public string displayName;
    public Sprite categoryIcon;
    
    [Header("Allowed Items")]
    public List<CollectableType> allowedItems = new List<CollectableType>();
    
    [Header("Category Settings")]
    [Tooltip("Giá tối thiểu cho items trong category này")]
    public int minPrice = 1;
    
    [Tooltip("Giá tối đa cho items trong category này")]
    public int maxPrice = 10000;
} 