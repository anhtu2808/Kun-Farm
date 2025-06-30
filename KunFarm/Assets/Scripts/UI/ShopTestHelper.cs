using UnityEngine;

/// <summary>
/// Helper script để test shop functionality và debug
/// </summary>
public class ShopTestHelper : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool enableDebugLogs = true;
    public KeyCode testSellKey = KeyCode.T;
    public KeyCode testBuyKey = KeyCode.B;
    
    [Header("References")]
    public ShopManager shopManager;
    public Player player;
    
    private void Start()
    {
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
            
        if (player == null)
            player = FindObjectOfType<Player>();
            
        if (enableDebugLogs)
        {
            Debug.Log("ShopTestHelper: Initialized for debugging");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(testSellKey))
        {
            TestSellFirstItem();
        }
        
        if (Input.GetKeyDown(testBuyKey))
        {
            TestBuyItem();
        }
    }
    
    /// <summary>
    /// Test bán item đầu tiên trong inventory
    /// </summary>
    private void TestSellFirstItem()
    {
        if (player == null || player.inventory == null || shopManager == null)
        {
            Debug.LogWarning("ShopTestHelper: Missing references for test");
            return;
        }
        
        // Tìm item đầu tiên có thể bán
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                ShopItem shopItem = shopManager.shopData?.GetShopItem(slot.type);
                if (shopItem != null && shopItem.canSell)
                {
                    Debug.Log($"ShopTestHelper: Testing sell of {slot.count}x {slot.type} from slot {i}");
                    bool success = shopManager.SellItem(i, 1);
                    Debug.Log($"ShopTestHelper: Sell result = {success}");
                    return;
                }
            }
        }
        
        Debug.Log("ShopTestHelper: No sellable items found in inventory");
    }
    
    /// <summary>
    /// Test mua item
    /// </summary>
    private void TestBuyItem()
    {
        if (shopManager == null || shopManager.shopData == null)
        {
            Debug.LogWarning("ShopTestHelper: Missing shop references for test");
            return;
        }
        
        // Tìm item đầu tiên có thể mua
        foreach (var shopItem in shopManager.shopData.shopItems)
        {
            if (shopItem.canBuy && shopItem.HasStock())
            {
                Debug.Log($"ShopTestHelper: Testing buy of {shopItem.itemName}");
                bool success = shopManager.BuyItem(shopItem.collectableType, 1);
                Debug.Log($"ShopTestHelper: Buy result = {success}");
                return;
            }
        }
        
        Debug.Log("ShopTestHelper: No buyable items found in shop");
    }
    
    /// <summary>
    /// Log current inventory state
    /// </summary>
    [ContextMenu("Log Inventory State")]
    public void LogInventoryState()
    {
        if (player == null || player.inventory == null)
        {
            Debug.LogWarning("ShopTestHelper: Cannot log inventory - missing references");
            return;
        }
        
        Debug.Log("=== INVENTORY STATE ===");
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type != CollectableType.NONE && slot.count > 0)
            {
                Debug.Log($"Slot {i}: {slot.count}x {slot.type}");
            }
        }
        Debug.Log($"Money: {player.wallet.Money}G");
        Debug.Log("=======================");
    }
    
    /// <summary>
    /// Log current shop state
    /// </summary>
    [ContextMenu("Log Shop State")]
    public void LogShopState()
    {
        if (shopManager == null || shopManager.shopData == null)
        {
            Debug.LogWarning("ShopTestHelper: Cannot log shop - missing references");
            return;
        }
        
        Debug.Log("=== SHOP STATE ===");
        foreach (var shopItem in shopManager.shopData.shopItems)
        {
            if (shopItem.showInShop)
            {
                Debug.Log($"{shopItem.itemName}: Buy={shopItem.canBuy}, Sell={shopItem.canSell}, Stock={shopItem.currentStock}/{shopItem.stockLimit}");
            }
        }
        Debug.Log("==================");
    }
} 