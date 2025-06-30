using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Quản lý logic marketplace online (player-to-player trading)
/// </summary>
public class MarketplaceManager : MonoBehaviour
{
    [Header("Marketplace Configuration")]
    public MarketplaceData marketplaceData;
    
    [Header("References")]
    public Player player;
    public ItemManager itemManager;
    public ApiClient apiClient;
    
    // Events
    public System.Action OnMarketplaceUpdated;
    public System.Action<List<MarketplaceItem>> OnItemsLoaded;
    public System.Action<string> OnError;
    
    // Cache
    private List<MarketplaceItem> activeItems = new List<MarketplaceItem>();
    private List<MarketplaceTransaction> userTransactions = new List<MarketplaceTransaction>();
    
    private void Awake()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (itemManager == null) itemManager = FindObjectOfType<ItemManager>();
        if (apiClient == null) apiClient = FindObjectOfType<ApiClient>();
    }
    
    /// <summary>
    /// Đăng bán item lên marketplace
    /// </summary>
    public async Task<bool> ListItem(CollectableType itemType, int quantity, int price, string description = "")
    {
        if (!ValidateListing(itemType, quantity, price)) return false;
        
        var request = new CreateMarketplaceItemRequest
        {
            ItemName = GetItemName(itemType),
            ItemId = (int)itemType,
            Quantity = quantity,
            Price = price,
            Description = description,
            ExpiryDate = DateTime.Now.AddDays(marketplaceData.defaultExpiryDays)
        };
        
        try
        {
            var response = await apiClient.CreateMarketplaceItem(request);
            if (response.Success && response.Data != null)
            {
                RemoveItemsFromInventory(itemType, quantity);
                await LoadActiveItems();
                OnMarketplaceUpdated?.Invoke();
                return true;
            }
            OnError?.Invoke(response.Message ?? "Lỗi khi đăng bán item");
            return false;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Mua item từ marketplace
    /// </summary>
    public async Task<bool> BuyItem(int marketplaceItemId, int quantity)
    {
        var item = activeItems.Find(i => i.id == marketplaceItemId);
        if (item == null || !item.IsActive() || quantity > item.quantity)
        {
            OnError?.Invoke("Item không hợp lệ");
            return false;
        }
        
        int totalPrice = item.price * quantity;
        if (player.wallet.Money < totalPrice)
        {
            OnError?.Invoke($"Không đủ tiền. Cần {totalPrice}G");
            return false;
        }
        
        var request = new BuyMarketplaceItemRequest
        {
            MarketplaceItemId = marketplaceItemId,
            Quantity = quantity
        };
        
        try
        {
            var response = await apiClient.BuyMarketplaceItem(request);
            if (response.Success && response.Data != null)
            {
                player.wallet.Spend(totalPrice);
                AddItemsToInventory(item.collectableType, quantity);
                await LoadActiveItems();
                OnMarketplaceUpdated?.Invoke();
                return true;
            }
            OnError?.Invoke(response.Message ?? "Lỗi khi mua item");
            return false;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Hủy item đã đăng bán
    /// </summary>
    public async Task<bool> CancelItem(int marketplaceItemId)
    {
        if (apiClient == null)
        {
            OnError?.Invoke("Missing API client reference");
            return false;
        }
        
        try
        {
            var response = await apiClient.CancelMarketplaceItem(marketplaceItemId);
            
            if (response.Success)
            {
                // Refresh marketplace
                await LoadActiveItems();
                
                OnMarketplaceUpdated?.Invoke();
                Debug.Log("Đã hủy item thành công");
                return true;
            }
            else
            {
                OnError?.Invoke(response.Message ?? "Lỗi khi hủy item");
                return false;
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load danh sách items đang hoạt động
    /// </summary>
    public async Task LoadActiveItems()
    {
        try
        {
            var response = await apiClient.GetActiveMarketplaceItems();
            if (response.Success && response.Data != null)
            {
                activeItems.Clear();
                foreach (var itemResponse in response.Data)
                {
                    var item = MarketplaceItem.FromApiResponse(itemResponse);
                    item.itemIcon = GetItemIcon(item.collectableType);
                    activeItems.Add(item);
                }
                OnItemsLoaded?.Invoke(activeItems);
            }
            else
            {
                OnError?.Invoke(response.Message ?? "Lỗi khi load items");
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load lịch sử giao dịch của user
    /// </summary>
    public async Task LoadUserTransactions()
    {
        if (apiClient == null)
        {
            OnError?.Invoke("Missing API client reference");
            return;
        }
        
        try
        {
            var response = await apiClient.GetUserMarketplaceTransactions();
            
            if (response.Success && response.Data != null)
            {
                userTransactions.Clear();
                foreach (var transactionResponse in response.Data)
                {
                    var transaction = MarketplaceTransaction.FromApiResponse(transactionResponse);
                    userTransactions.Add(transaction);
                }
                
                OnMarketplaceUpdated?.Invoke();
                Debug.Log($"Loaded {userTransactions.Count} user transactions");
            }
            else
            {
                OnError?.Invoke(response.Message ?? "Lỗi khi load transactions");
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Tìm kiếm items
    /// </summary>
    public async Task<List<MarketplaceItem>> SearchItems(string searchTerm)
    {
        if (apiClient == null)
        {
            OnError?.Invoke("Missing API client reference");
            return new List<MarketplaceItem>();
        }
        
        try
        {
            var response = await apiClient.SearchMarketplaceItems(searchTerm);
            
            if (response.Success && response.Data != null)
            {
                var searchResults = new List<MarketplaceItem>();
                foreach (var itemResponse in response.Data)
                {
                    var item = MarketplaceItem.FromApiResponse(itemResponse);
                    item.itemIcon = GetItemIcon(item.collectableType);
                    item.category = marketplaceData?.GetItemCategory(item.collectableType) ?? "Unknown";
                    searchResults.Add(item);
                }
                
                return searchResults;
            }
            else
            {
                OnError?.Invoke(response.Message ?? "Lỗi khi tìm kiếm");
                return new List<MarketplaceItem>();
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Lỗi kết nối: {ex.Message}");
            return new List<MarketplaceItem>();
        }
    }
    
    /// <summary>
    /// Validate listing request
    /// </summary>
    private bool ValidateListing(CollectableType itemType, int quantity, int price)
    {
        if (!marketplaceData.CanListItem(itemType))
        {
            OnError?.Invoke($"Item {itemType} không thể đăng bán");
            return false;
        }
        
        if (GetPlayerItemCount(itemType) < quantity)
        {
            OnError?.Invoke($"Không đủ {itemType} để đăng bán");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Lấy số lượng item trong inventory
    /// </summary>
    public int GetPlayerItemCount(CollectableType itemType)
    {
        if (player?.inventory == null) return 0;
        
        int count = 0;
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == itemType) count += slot.count;
        }
        return count;
    }
    
    /// <summary>
    /// Xóa items từ inventory
    /// </summary>
    private void RemoveItemsFromInventory(CollectableType itemType, int quantity)
    {
        if (player?.inventory == null) return;
        
        int remaining = quantity;
        for (int i = player.inventory.slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == itemType && slot.count > 0)
            {
                int toRemove = Mathf.Min(remaining, slot.count);
                for (int j = 0; j < toRemove; j++)
                {
                    slot.RemoveItem();
                }
                remaining -= toRemove;
            }
        }
    }
    
    /// <summary>
    /// Thêm items vào inventory
    /// </summary>
    private void AddItemsToInventory(CollectableType itemType, int quantity)
    {
        if (player?.inventory == null) return;
        
        Collectable itemPrefab = itemManager?.GetItemByType(itemType);
        for (int i = 0; i < quantity; i++)
        {
            player.inventory.AddItemByType(itemType, itemPrefab?.icon, 1);
        }
    }
    
    /// <summary>
    /// Helper methods
    /// </summary>
    private string GetItemName(CollectableType itemType) => itemType.ToString();
    private Sprite GetItemIcon(CollectableType itemType) => itemManager?.GetItemByType(itemType)?.icon;
    public List<MarketplaceItem> GetActiveItems() => activeItems;
    public List<MarketplaceTransaction> GetUserTransactions() => userTransactions;
} 