using UnityEngine;

/// <summary>
/// Quản lý logic mua/bán items trong shop
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Configuration")]
    public ShopData shopData;
      
    [Header("References")]
    public Player player;
    public ItemManager itemManager;
    
    // Events
    public System.Action OnShopUpdated;
    
    private void Awake()
    {
      // Tự động tìm references nếu chưa assign
        if (player == null)
            player = FindObjectOfType<Player>();
            
        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();
    }

    /// <summary>
    /// Mua item từ shop
    /// </summary>
    public bool BuyItem(CollectableType itemType, int quantity = 1)
    {
        if (shopData == null || player == null)
        {
            Debug.LogError("ShopData hoặc Player chưa được assign!");
            return false;
        }

        ShopItem shopItem = shopData.GetShopItem(itemType);
        if (shopItem == null)
        {
            Debug.LogWarning($"Item {itemType} không có trong shop!");
            return false;
        }

        // Kiểm tra có thể mua không
        if (!shopItem.canBuy || !shopItem.showInShop)
        {
            Debug.LogWarning($"Item {itemType} không thể mua!");
            return false;
        }

        // Kiểm tra stock
        if (!shopItem.HasStock())
        {
            Debug.LogWarning($"Item {itemType} đã hết hàng!");
            return false;
        }

        // Kiểm tra quantity với stock
        if (shopItem.stockLimit >= 0 && quantity > shopItem.currentStock)
        {
            Debug.LogWarning($"Không đủ hàng! Chỉ còn {shopItem.currentStock} {shopItem.itemName}");
            return false;
        }

        // Tính tổng giá
        int totalPrice = shopItem.buyPrice * quantity;
        
        // Kiểm tra tiền
        if (player.wallet.Money < totalPrice)
        {
            Debug.LogWarning($"Không đủ tiền! Cần {totalPrice}G, chỉ có {player.wallet.Money}G");
            return false;
        }

        // Thực hiện mua
        if (player.wallet.Spend(totalPrice))
        {
            // Thêm item vào inventory
            Collectable itemPrefab = itemManager.GetItemByType(itemType);
            if (itemPrefab != null)
            {
                // Thêm multiple items nếu quantity > 1
                for (int i = 0; i < quantity; i++)
                {
                    player.inventory.AddItemByType(itemType, itemPrefab.icon, 1);
                }
            }
            else
            {
                // Fallback: add without icon nếu không tìm thấy prefab
                player.inventory.AddItemByType(itemType, shopItem.itemIcon, quantity);
            }

            // Giảm stock
            shopItem.ConsumeStock(quantity);
            
            Debug.Log($"Đã mua {quantity} {shopItem.itemName} với giá {totalPrice}G");
            
            // Trigger event
            OnShopUpdated?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Bán item từ inventory
    /// </summary>
    public bool SellItem(int inventorySlotIndex, int quantity = 1)
    {
        if (player == null || player.inventory == null)
        {
            Debug.LogError("Player hoặc Inventory chưa được assign!");
            return false;
        }

        if (inventorySlotIndex < 0 || inventorySlotIndex >= player.inventory.slots.Count)
        {
            Debug.LogError("Invalid inventory slot index!");
            return false;
        }

        Inventory.Slot slot = player.inventory.slots[inventorySlotIndex];
        
        // Kiểm tra slot có item không
        if (slot.type == CollectableType.NONE || slot.count <= 0)
        {
            Debug.LogWarning("Slot này không có item để bán!");
            return false;
        }

        // Kiểm tra quantity
        if (quantity > slot.count)
        {
            Debug.LogWarning($"Không đủ item để bán! Chỉ có {slot.count}");
            return false;
        }

        ShopItem shopItem = shopData.GetShopItem(slot.type);
        if (shopItem == null)
        {
            Debug.LogWarning($"Item {slot.type} không thể bán!");
            return false;
        }

        // Kiểm tra có thể bán không
        if (!shopItem.canSell)
        {
            Debug.LogWarning($"Item {shopItem.itemName} không thể bán!");
            return false;
        }

        // Tính tổng giá bán
        int totalSellPrice = shopItem.sellPrice * quantity;
        
        // Thực hiện bán
        player.wallet.Add(totalSellPrice);
        
        // Xóa item từ inventory
        for (int i = 0; i < quantity; i++)
        {
            slot.RemoveItem();
        }
        
        Debug.Log($"Đã bán {quantity} {shopItem.itemName} với giá {totalSellPrice}G");
        
        // Trigger event
        OnShopUpdated?.Invoke();
        return true;
    }

    /// <summary>
    /// Bán tất cả items cùng loại từ inventory
    /// </summary>
    public bool SellAllItems(CollectableType itemType)
    {
        if (player == null || player.inventory == null)
            return false;

        int totalQuantity = 0;
        int totalValue = 0;
        
        ShopItem shopItem = shopData.GetShopItem(itemType);
        if (shopItem == null || !shopItem.canSell)
        {
            Debug.LogWarning($"Item {itemType} không thể bán!");
            return false;
        }

        // Đếm tổng số items cùng loại
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == itemType)
            {
                totalQuantity += slot.count;
            }
        }

        if (totalQuantity <= 0)
        {
            Debug.LogWarning($"Không có {shopItem.itemName} nào để bán!");
            return false;
        }

        totalValue = totalQuantity * shopItem.sellPrice;
        
        // Xóa tất cả items cùng loại
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == itemType)
            {
                slot.count = 0;
                slot.type = CollectableType.NONE;
                slot.icon = null;
            }
        }

        // Thêm tiền
        player.wallet.Add(totalValue);
        
        // Trigger inventory update
        player.inventory.NotifyInventoryChanged();
        
        Debug.Log($"Đã bán tất cả {totalQuantity} {shopItem.itemName} với giá {totalValue}G");
        
        OnShopUpdated?.Invoke();
        return true;
    }

    /// <summary>
    /// Refresh shop stock (có thể gọi hàng ngày)
    /// </summary>
    public void RefreshShopStock()
    {
        if (shopData == null) return;

        foreach (var item in shopData.shopItems)
        {
            item.RestoreStock();
        }
        
        OnShopUpdated?.Invoke();
        Debug.Log("Shop stock đã được refresh!");
    }

    /// <summary>
    /// Lấy số lượng item trong inventory
    /// </summary>
    public int GetInventoryItemCount(CollectableType itemType)
    {
        if (player == null || player.inventory == null)
            return 0;

        int count = 0;
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == itemType)
            {
                count += slot.count;
            }
        }
        return count;
    }
} 