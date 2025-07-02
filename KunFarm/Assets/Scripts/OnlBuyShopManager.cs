using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OnlBuyShopManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameObject shopPanel;
    public ItemManager itemManager;
    public Transform shopSlotContainer;
    public GameObject shopSlotPrefab;
    public PlayerInventoryScroll_UI playerInventoryScrollUI;
    
    [Header("Settings")]
    public int playerId = 0; // Will be loaded from PlayerPrefs
    
    // Events
    public System.Action OnShopUpdated;
    private bool isOpen = false;

    // Removed old batching logic - now buy immediately
    // private bool hasBuyItem = false;
    // private List<int> list;

    private void Awake()
    {
        // Tự động tìm references nếu chưa assign
        if (player == null)
            player = FindObjectOfType<Player>();

        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();

        if (playerInventoryScrollUI == null)
            playerInventoryScrollUI = FindObjectOfType<PlayerInventoryScroll_UI>(); // Nếu bạn chưa gán thủ công
        
        // Load player ID từ PlayerPrefs
        playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId > 0)
        {
            Debug.Log($"[OnlBuyShopManager] Loaded player ID from PlayerPrefs: {playerId}");
        }
        else
        {
            Debug.LogWarning("[OnlBuyShopManager] No valid player ID found in PlayerPrefs");
        }
        
        shopPanel.SetActive(false);
        if (playerId > 0)
        {
            StartCoroutine(LoadShopData());
        }
    }

    void Update()
    {
        // Input handling moved to UIManager
        // P key is now handled by UIManager
    }

    public void ToggleShop()
    {
        // Removed old batching logic - items are now bought immediately via API
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
    }

    public void CloseShop()
    {

        isOpen = false;
        shopPanel.SetActive(false);

    }

    public void OpenShop()
    {
        isOpen = true;
        shopPanel.SetActive(true);
        
        // Load fresh shop data từ API
        LoadShopDataOnOpen();
        
        // Refresh inventory UI để hiển thị items hiện tại
        if (playerInventoryScrollUI != null)
    {
            StartCoroutine(RefreshInventoryWithDelay());
        }
    }

    /// <summary>
    /// Refresh inventory với delay nhỏ để đảm bảo UI đã active
    /// </summary>
    private System.Collections.IEnumerator RefreshInventoryWithDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // Double frame wait for safety
        if (playerInventoryScrollUI != null)
        {
            playerInventoryScrollUI.RefreshInventoryUI();
        }
    }

    private IEnumerator LoadShopData(int customPlayerId = -1)
    {
        int targetPlayerId = customPlayerId > 0 ? customPlayerId : playerId;
        string apiUrl = $"http://localhost:5270/online-shop/2";

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"✅ [Online Buy] Shop data received: {json}");
            
            BuyShopWrapper response = JsonUtility.FromJson<BuyShopWrapper>(json);
            if (response?.data != null)
            {
                Debug.Log($"📊 [Online Buy] Loading {response.data.Count()} shop items");

                foreach (var item in response.data)
                {
                    Debug.Log($"📦 [Online Buy] Shop item: {item.collectableType} - {item.price}G - CanBuy: {item.canBuy} - Qty: {item.quantity}");
                GameObject slotGO = Instantiate(shopSlotPrefab, shopSlotContainer);
                var slotUI = slotGO.GetComponent<ShopBuySlot_UI>();
                slotUI.Setup(item, this);
                }
                
                Debug.Log($"✅ [Online Buy] Loaded {response.data.Count()} shop items successfully");
            }
            else
            {
                Debug.Log("📋 [Online Buy] No shop data available or empty response");
            }
        }
        else
        {
            Debug.LogError("API lỗi: " + request.error);
            SimpleNotificationPopup.Show($"Không thể tải dữ liệu shop! Lỗi: {request.error}");
        }
        // Removed refresh call from here - now done in OpenShop()
    }

    /// <summary>
    /// Mua item từ shop - call API ngay lập tức
    /// </summary>
    public void BuyItem(SellItemResponse data)
    {
        // Validation trước khi mua
        if (data == null || !data.canBuy)
        {
            Debug.LogWarning($"❌ [Online Buy] Item không thể mua: {data?.collectableType}");
            return;
        }

        // Kiểm tra đủ tiền
        if (player?.wallet == null)
        {
            Debug.LogError("❌ [Online Buy] Player wallet không tìm thấy!");
            return;
        }

        if (player.wallet.Money < data.price)
        {
            Debug.LogWarning($"❌ [Online Buy] Không đủ tiền! Cần: {data.price}G, Có: {player.wallet.Money}G");
            SimpleNotificationPopup.Show($"Không đủ tiền! Cần: {data.price}G, bạn có: {player.wallet.Money}G");
            return;
            }

        Debug.Log($"🛒 [Online Buy] Mua item: {data.collectableType}, Số lượng: {data.quantity}, Giá: {data.price}G");
        
        // Call API ngay lập tức để update DB
        StartCoroutine(SendBuyRequestImmediate(data));
    }

    /// <summary>
    /// Gửi buy request ngay lập tức cho 1 item
    /// </summary>
    private IEnumerator SendBuyRequestImmediate(SellItemResponse data)
    {
        if (playerId <= 0)
        {
            Debug.LogError("[OnlBuyShopManager] No valid player ID for buy request");
            yield break;
        }
        string apiUrl = $"http://localhost:5270/online-shop/buy/{playerId}";
        List<int> itemIds = new List<int> { data.id };

        string json = "[" + string.Join(",", itemIds) + "]";
        Debug.Log($"📤 [Online Buy] Gửi yêu cầu mua itemId: {data.id} cho playerId: {playerId}, JSON: {json}");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ [Online Buy] Mua thành công itemId: {data.id} cho playerId: {playerId}! Response: " + request.downloadHandler.text);
            
            // ✅ TRỪ TIỀN KHI API THÀNH CÔNG
            bool moneySpent = player.wallet.Spend(data.price);
            if (!moneySpent)
            {
                Debug.LogError($"❌ [Online Buy] Không thể trừ tiền! Cần: {data.price}G, Có: {player.wallet.Money}G");
                yield break;
            }
            
            Debug.Log($"💰 [Online Buy] Đã trừ {data.price}G, còn lại: {player.wallet.Money}G");
            SimpleNotificationPopup.Show($"Mua thành công {data.collectableType} với giá {data.price}G! Còn lại: {player.wallet.Money}G");
            
            // CHỈ ADD VÀO INVENTORY NẾU API THÀNH CÔNG VÀ ĐÃ TRỪ TIỀN
            if (Enum.TryParse<CollectableType>(data.collectableType, ignoreCase: true, out var parsedType))
            {
                var collectable = itemManager.GetItemByType(parsedType);
                if (collectable != null)
                {
                    player.inventory.Add(collectable, data.quantity);
                    player.inventory.NotifyInventoryChanged();
                    Debug.Log($"✅ [Online Buy] Đã thêm {data.quantity}x {parsedType} vào inventory");
                    SimpleNotificationPopup.Show($"Đã thêm {data.quantity}x {parsedType} vào inventory!");
                }
                else
                {
                    Debug.LogError($"❌ [Online Buy] Không tìm thấy collectable cho {parsedType}");
                }
            }
            else
            {
                Debug.LogError($"❌ [Online Buy] Không parse được CollectableType từ '{data.collectableType}'");
            }
            
            // Hide item UI sau khi mua thành công
            HideItemAfterPurchase(data.id);
        }
        else
        {
            Debug.LogError($"❌ [Online Buy] Mua thất bại itemId: {data.id}, Error: " + request.error);
            SimpleNotificationPopup.Show($"Mua thất bại! Lỗi: {request.error}");
        }
    }

    /// <summary>
    /// Ẩn item UI sau khi mua thành công
    /// </summary>
    private void HideItemAfterPurchase(int itemId)
    {
        // Tìm và ẩn UI slot tương ứng với itemId
        foreach (Transform child in shopSlotContainer)
        {
            var slotUI = child.GetComponent<ShopBuySlot_UI>();
            if (slotUI != null && slotUI.GetItemId() == itemId)
            {
                child.gameObject.SetActive(false);
                break;
            }
        }
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

    void OnEnable()
    {
        // Load lại dữ liệu shop mỗi lần mở
        LoadShopDataOnOpen();
    }
    
    void OnDisable()
    {
        // Cleanup if needed
    }

    /// <summary>
    /// Load lại dữ liệu shop mỗi lần mở giao diện
    /// </summary>
    private void LoadShopDataOnOpen()
    {
        Debug.Log("📥 [Online Buy] Refresh shop data khi mở giao diện");
        
        // Clear existing items
        ClearShopItems();
        
        // Load fresh data from API
        StartCoroutine(LoadShopData());
    }
    
    /// <summary>
    /// Clear all existing shop items from UI
    /// </summary>
    private void ClearShopItems()
    {
        if (shopSlotContainer != null)
        {
            foreach (Transform child in shopSlotContainer)
            {
                if (child.gameObject != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        Debug.Log("🧹 [Online Buy] Đã clear tất cả items cũ");
    }
}
