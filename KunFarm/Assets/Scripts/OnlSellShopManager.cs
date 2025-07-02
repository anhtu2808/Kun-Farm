using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OnlSellShopManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameObject shopPanel;
    public ItemManager itemManager;
    public PlayerInventoryScroll_UI playerInventoryScrollUI;

    [Header("Settings")]
    public int playerId = 1; // Default player ID, có thể set từ inspector hoặc script khác

    [Header("UI")]
    public Transform sellSlotContainer;
    public GameObject shopSellSlotPrefab;
    public GameObject soldItemSlotPrefab; // Prefab cho items đã đăng bán

    private List<SoldItemSlot_UI> soldSlots = new();
    private bool isOpen = false;
    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();
        if (playerInventoryScrollUI == null)
            playerInventoryScrollUI = FindObjectOfType<PlayerInventoryScroll_UI>();
        shopPanel.SetActive(false);
    }

    void Update()
    {
        // Input handling moved to UIManager
        // O key is now handled by UIManager
    }

    public void ToggleShop()
    {
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
        
        // Refresh inventory UI để hiển thị items hiện tại
        if (playerInventoryScrollUI != null)
        {
            StartCoroutine(RefreshInventoryWithDelayCoroutine());
        }
        
        // Load sold items để hiển thị trên UI (không auto claim)
        LoadSoldItemsForDisplay();
    }

    /// <summary>
    /// Refresh inventory với delay nhỏ để đảm bảo UI đã active
    /// </summary>
    public void RefreshInventoryWithDelay()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshInventoryWithDelayCoroutine());
        }
        else
        {
            // Fallback: gọi trực tiếp refresh
            if (playerInventoryScrollUI != null)
            {
                playerInventoryScrollUI.RefreshInventoryUI();
            }
        }
    }

    private IEnumerator RefreshInventoryWithDelayCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // Double frame wait for safety
        if (playerInventoryScrollUI != null)
        {
            playerInventoryScrollUI.RefreshInventoryUI();
        }
    }

    /// <summary>
    /// Gọi khi người chơi muốn bán item lên shop online
    /// </summary>
    public void SellItemOnline(CollectableType type, int quantity, int pricePerUnit)
    {
        // Kiểm tra xem player có đủ item để bán không
        if (!HasSufficientItems(type, quantity))
        {
            Debug.LogWarning($"❌ [Online Sell] Không đủ {type} để bán! Cần: {quantity}, Có: {GetInventoryItemCount(type)}");
            return;
        }

        Debug.Log($"🛒 [Online Sell] Bán item: {type}, Số lượng: {quantity}, Giá mỗi đơn vị: {pricePerUnit}G");
        int totalPrice = quantity * pricePerUnit;
        StartCoroutine(SendSellRequest(type.ToString(), quantity, totalPrice, type));
    }

    private IEnumerator SendSellRequest(string collectableType, int quantity, int totalPrice, CollectableType itemType)
    {
        Debug.Log($"📤 [Online Sell] Gửi yêu cầu bán: {collectableType}, Số lượng: {quantity}, Tổng giá: {totalPrice}");
        string url = $"http://localhost:5270/online-shop/sell/{1}";

        SellItemRequest requestData = new SellItemRequest
        {
            collectableType = collectableType,
            quantity = quantity,
            price = totalPrice
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("📤 [Online Sell] Đang gửi dữ liệu: " + json);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ [Online Sell] Bán thành công: " + request.downloadHandler.text);
            
            // ✅ THỰC SỰ TRỪ ITEM KHỎI INVENTORY
            bool itemRemoved = RemoveItemFromInventory(itemType, quantity);
            if (itemRemoved)
            {
                CollectableType parsedType = (CollectableType)System.Enum.Parse(typeof(CollectableType), collectableType);
                AddSoldItem(parsedType, quantity, totalPrice);
                
                // Notify inventory changed để refresh UI
                if (player != null && player.inventory != null)
                {
                    player.inventory.NotifyInventoryChanged();
                }
                
                Debug.Log($"✅ [Online Sell] Đã trừ {quantity}x {itemType} khỏi inventory");
            }
            else
            {
                Debug.LogError($"❌ [Online Sell] Không thể trừ {quantity}x {itemType} khỏi inventory!");
            }
        }
        else
        {
            Debug.LogError("❌ [Online Sell] Lỗi khi gửi request: " + request.error);
        }
    }

    /// <summary>
    /// Trừ item khỏi inventory của player
    /// </summary>
    private bool RemoveItemFromInventory(CollectableType itemType, int quantity)
    {
        if (player == null || player.inventory == null)
        {
            Debug.LogError("OnlSellShopManager: Player hoặc inventory null!");
            return false;
        }

        // Tìm item trong inventory và trừ đi
        int remainingToRemove = quantity;
        for (int i = 0; i < player.inventory.slots.Count && remainingToRemove > 0; i++)
        {
            var slot = player.inventory.slots[i];
            if (slot.type == itemType && slot.count > 0)
            {
                int removeFromThisSlot = Mathf.Min(remainingToRemove, slot.count);
                
                // Trừ từ slot này
                player.inventory.slots[i] = new Inventory.Slot
                {
                    type = slot.count - removeFromThisSlot <= 0 ? CollectableType.NONE : slot.type,
                    count = Mathf.Max(0, slot.count - removeFromThisSlot),
                    icon = slot.count - removeFromThisSlot <= 0 ? null : slot.icon
                };
                
                remainingToRemove -= removeFromThisSlot;
                Debug.Log($"Trừ {removeFromThisSlot}x {itemType} từ slot {i}, còn lại: {player.inventory.slots[i].count}");
            }
        }

        return remainingToRemove == 0; // True nếu trừ hết số lượng cần thiết
    }

    /// <summary>
    /// Kiểm tra xem có đủ item để bán không
    /// </summary>
    private bool HasSufficientItems(CollectableType itemType, int requiredQuantity)
    {
        return GetInventoryItemCount(itemType) >= requiredQuantity;
    }

    /// <summary>
    /// Lấy tổng số lượng item trong inventory
    /// </summary>
    private int GetInventoryItemCount(CollectableType itemType)
    {
        if (player == null || player.inventory == null)
            return 0;

        int totalCount = 0;
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == itemType)
            {
                totalCount += slot.count;
            }
        }
        return totalCount;
    }

    private void AddSoldItem(CollectableType itemType, int quantity, int totalPrice)
    {
        // ❌ OLD METHOD - Không dùng nữa vì giờ ta load từ server
        // Method này sẽ bị deprecated, ta chỉ cần refresh từ server
        Debug.LogWarning("[AddSoldItem] Method deprecated - use LoadSoldItemsForDisplay() instead");
        
        // Refresh data từ server thay vì add local
        LoadSoldItemsForDisplay();
    }

    /// <summary>
    /// Làm mới danh sách (ẩn tất cả slot)
    /// </summary>
    private void ClearAllSlots()
    {
        foreach (var slot in soldSlots)
        {
            Destroy(slot.gameObject);
        }
        soldSlots.Clear();
    }

    /// <summary>
    /// Load danh sách items player đã bán để hiển thị trên UI
    /// </summary>
    public void LoadSoldItemsForDisplay()
    {
        StartCoroutine(LoadSoldItemsCoroutine());
    }

    private IEnumerator LoadSoldItemsCoroutine()
    {
        string apiUrl = $"http://localhost:5270/online-shop/sold-items/{playerId}";
        
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"✅ [Sell Shop] Sold items response: {json}");
            
            // Parse response
            SoldItemsWrapper response = JsonUtility.FromJson<SoldItemsWrapper>(json);
            if (response != null && response.data != null)
            {
                Debug.Log($"📋 [Sell Shop] Tìm thấy {response.data.Count} items đã đăng bán");
                
                // Hiển thị tất cả items trên UI (cả đang bán và đã bán)
                DisplaySoldItemsOnUI(response.data);
            }
        }
        else
        {
            Debug.LogError($"❌ [Sell Shop] Load sold items failed: {request.error}");
        }
    }

    /// <summary>
    /// Hiển thị danh sách sold items lên UI với button claim riêng biệt
    /// </summary>
    private void DisplaySoldItemsOnUI(List<SellItemResponse> soldItems)
    {
        // Clear existing UI slots first
        ClearAllSlots();
        
        foreach (var item in soldItems)
        {
            // Sử dụng soldItemSlotPrefab thay vì shopSellSlotPrefab
            GameObject slotGO = Instantiate(soldItemSlotPrefab, sellSlotContainer);
            var slotUI = slotGO.GetComponent<SoldItemSlot_UI>();
            
            if (slotUI != null)
            {
                // Setup UI với callback claim
                slotUI.Setup(item, this, OnClaimSingleItem);
                Debug.Log($"📦 [Sell Shop UI] Added item: {item.collectableType}, Price: {item.price}G, CanBuy: {item.canBuy}");
            }
            else
            {
                Debug.LogError($"❌ [Sell Shop UI] SoldItemSlot_UI component not found on prefab!");
            }
        }
    }

    /// <summary>
    /// Callback khi player click claim một item cụ thể
    /// </summary>
    private void OnClaimSingleItem(SellItemResponse item)
    {
        if (item.canBuy)
        {
            Debug.LogWarning($"⚠️ [Claim] Item {item.collectableType} vẫn đang bán (canBuy=true), không thể claim!");
            return;
        }
        
        Debug.Log($"💰 [Claim] Player muốn claim: {item.collectableType} với giá {item.price}G");
        
        // Claim chỉ 1 item này
        List<int> singleItemId = new List<int> { item.id };
        StartCoroutine(ClaimMoneyCoroutine(singleItemId, item.price));
    }

    /// <summary>
    /// Claim money từ các items đã bán
    /// </summary>
    private IEnumerator ClaimMoneyCoroutine(List<int> itemIds, int expectedAmount)
    {
        string apiUrl = $"http://localhost:5270/online-shop/claim-money/{playerId}";
        string json = "[" + string.Join(",", itemIds) + "]";
        
        Debug.Log($"📤 [Sell Shop] Claiming money for items: {json}");
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ [Sell Shop] Claim money thành công! Response: {request.downloadHandler.text}");
            
            // Cộng tiền vào wallet Unity
            if (player?.wallet != null)
            {
                player.wallet.Add(expectedAmount);
                Debug.Log($"💰 [Sell Shop] Đã cộng {expectedAmount}G vào wallet, tổng: {player.wallet.Money}G");
            }
            
            // Refresh lại danh sách items sau khi claim
            LoadSoldItemsForDisplay();
            
            Debug.Log($"🎉 [Sell Shop] Claim thành công {expectedAmount}G!");
        }
        else
        {
            Debug.LogError($"❌ [Sell Shop] Claim money failed: {request.error}");
            
            // Re-enable claim button for failed items
            EnableClaimButtonsForItems(itemIds);
        }
    }

    /// <summary>
    /// Re-enable claim buttons for specific items (khi claim thất bại)
    /// </summary>
    private void EnableClaimButtonsForItems(List<int> itemIds)
    {
        // Tìm và re-enable buttons cho các items failed
        foreach (Transform child in sellSlotContainer)
        {
            var slotUI = child.GetComponent<SoldItemSlot_UI>();
            if (slotUI != null)
            {
                slotUI.EnableClaimButton();
            }
        }
    }

    // Events
    public System.Action OnShopUpdated;
}

[System.Serializable]
public class SoldItemsWrapper
{
    public int code;
    public List<SellItemResponse> data;
    public string message;
}
