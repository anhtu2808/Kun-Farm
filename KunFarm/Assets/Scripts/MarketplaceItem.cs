using UnityEngine;
using System;

/// <summary>
/// Dữ liệu của một item trong marketplace online
/// </summary>
[System.Serializable]
public class MarketplaceItem
{
    [Header("Item Info")]
    public int id;
    public CollectableType collectableType;
    public string itemName;
    public Sprite itemIcon;
    public string description;
    
    [Header("Seller Info")]
    public int sellerId;
    public string sellerName;
    
    [Header("Listing Info")]
    public int quantity;
    public int price;
    public DateTime expiryDate;
    public bool isActive;
    public DateTime createdAt;
    
    [Header("Category")]
    public string category;
    
    /// <summary>
    /// Kiểm tra item có còn hạn không
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.Now > expiryDate;
    }
    
    /// <summary>
    /// Kiểm tra item có còn hoạt động không
    /// </summary>
    public bool IsActive()
    {
        return isActive && !IsExpired() && quantity > 0;
    }
    
    /// <summary>
    /// Tính tổng giá trị
    /// </summary>
    public int GetTotalValue()
    {
        return price * quantity;
    }
    
    /// <summary>
    /// Tạo từ API response
    /// </summary>
    public static MarketplaceItem FromApiResponse(MarketplaceItemResponse response)
    {
        return new MarketplaceItem
        {
            id = response.Id,
            collectableType = GetCollectableTypeFromName(response.ItemName),
            itemName = response.ItemName,
            description = response.Description,
            sellerId = response.SellerId,
            sellerName = response.SellerName,
            quantity = response.Quantity,
            price = response.Price,
            expiryDate = response.ExpiryDate,
            isActive = response.IsActive,
            createdAt = response.CreatedAt,
            category = "Unknown" // Sẽ được set sau
        };
    }
    
    /// <summary>
    /// Convert sang API request
    /// </summary>
    public CreateMarketplaceItemRequest ToApiRequest()
    {
        return new CreateMarketplaceItemRequest
        {
            ItemName = itemName,
            ItemId = (int)collectableType,
            Quantity = quantity,
            Price = price,
            Description = description,
            ExpiryDate = expiryDate
        };
    }
    
    /// <summary>
    /// Helper method để convert tên item thành CollectableType
    /// </summary>
    private static CollectableType GetCollectableTypeFromName(string itemName)
    {
        // Implement logic để convert tên item thành CollectableType
        // Có thể dùng dictionary hoặc switch case
        switch (itemName.ToLower())
        {
            case "apple":
                return CollectableType.APPLE;
            case "grape":
                return CollectableType.GRAPE;
            case "wheat":
                return CollectableType.WHEAT;
            case "egg":
                return CollectableType.EGG;
            default:
                return CollectableType.NONE;
        }
    }
}

/// <summary>
/// API Response model cho marketplace item
/// </summary>
[System.Serializable]
public class MarketplaceItemResponse
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; }
    public string ItemName { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// API Request model cho tạo marketplace item
/// </summary>
[System.Serializable]
public class CreateMarketplaceItemRequest
{
    public string ItemName { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public string Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// API Request model cho mua marketplace item
/// </summary>
[System.Serializable]
public class BuyMarketplaceItemRequest
{
    public int MarketplaceItemId { get; set; }
    public int Quantity { get; set; }
} 