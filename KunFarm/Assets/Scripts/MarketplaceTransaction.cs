using UnityEngine;
using System;

/// <summary>
/// Dữ liệu giao dịch marketplace
/// </summary>
[System.Serializable]
public class MarketplaceTransaction
{
    [Header("Transaction Info")]
    public int id;
    public int marketplaceItemId;
    public int buyerId;
    public string buyerName;
    public int sellerId;
    public string sellerName;
    public int quantity;
    public int totalPrice;
    public DateTime transactionDate;
    public TransactionStatus status;
    public string itemName;
    
    /// <summary>
    /// Tạo từ API response
    /// </summary>
    public static MarketplaceTransaction FromApiResponse(MarketplaceTransactionResponse response)
    {
        return new MarketplaceTransaction
        {
            id = response.Id,
            marketplaceItemId = response.MarketplaceItemId,
            buyerId = response.BuyerId,
            buyerName = response.BuyerName,
            sellerId = response.SellerId,
            sellerName = response.SellerName,
            quantity = response.Quantity,
            totalPrice = response.TotalPrice,
            transactionDate = response.TransactionDate,
            status = response.Status,
            itemName = response.ItemName
        };
    }
}

/// <summary>
/// API Response model cho marketplace transaction
/// </summary>
[System.Serializable]
public class MarketplaceTransactionResponse
{
    public int Id { get; set; }
    public int MarketplaceItemId { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; }
    public int Quantity { get; set; }
    public int TotalPrice { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionStatus Status { get; set; }
    public string ItemName { get; set; }
}

/// <summary>
/// Trạng thái giao dịch
/// </summary>
public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2,
    Failed = 3
} 