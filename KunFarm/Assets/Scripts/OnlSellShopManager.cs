using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class OnlSellShopManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameObject shopPanel;
    public ItemManager itemManager;
    public PlayerInventoryScroll_UI playerInventoryScrollUI;
    public Transform sellSlotContainer;

    [Header("Settings")]
    public int playerId = 0; // Will be loaded from PlayerPrefs

    [Header("UI")]
    public GameObject shopSellSlotPrefab;
    public GameObject soldItemSlotPrefab; // Prefab cho items đã đăng bán

    [Header("Slot Settings")]
    [SerializeField] private int minSlotCount = 27; // Số slot tối thiểu

    private List<SoldItemSlot_UI> soldSlots = new(); // Danh sách tất cả slot UI
    private bool isOpen = false;
    private bool slotsInitialized = false;

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();
        if (playerInventoryScrollUI == null)
            playerInventoryScrollUI = FindObjectOfType<PlayerInventoryScroll_UI>();
        
        // Load player ID từ PlayerPrefs
        playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId > 0)
        {
            Debug.Log($"[OnlSellShopManager] Loaded player ID from PlayerPrefs: {playerId}");
        }
        else
        {
            Debug.LogWarning("[OnlSellShopManager] No valid player ID found in PlayerPrefs");
        }
        
        shopPanel.SetActive(false);

        // Khởi tạo sẵn các slot rỗng
        InitializeEmptySlots();
    }

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    /// <summary>
    /// Khởi tạo sẵn 27 slot rỗng
    /// </summary>
    private void InitializeEmptySlots()
    {
        if (slotsInitialized || soldItemSlotPrefab == null || sellSlotContainer == null)
            return;

        Debug.Log($"🔧 [Sell Shop] Khởi tạo {minSlotCount} slot rỗng...");

        for (int i = 0; i < minSlotCount; i++)
        {
            GameObject slotGO = Instantiate(soldItemSlotPrefab, sellSlotContainer);
            var slotUI = slotGO.GetComponent<SoldItemSlot_UI>();
            
            if (slotUI != null)
            {
                slotUI.SetupEmptySlot(); // Setup slot rỗng
                soldSlots.Add(slotUI);
            }
            else
            {
                Destroy(slotGO);
            }
        }

        slotsInitialized = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (isOpen) CloseShop();
        else OpenShop();
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
        
        // Đảm bảo slots đã được khởi tạo
        if (!slotsInitialized)
        {
            InitializeEmptySlots();
        }
        
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
        if (!HasSufficientItems(type, quantity))
        {
            SimpleNotificationPopup.Show($"Không đủ {type} để bán! Cần: {quantity}, bạn có: {GetInventoryItemCount(type)}");
            return;
        }

        int totalPrice = quantity * pricePerUnit;
        StartCoroutine(SendSellRequest(type.ToString(), quantity, totalPrice, type));
    }

    private IEnumerator SendSellRequest(string collectableType, int quantity, int totalPrice, CollectableType itemType)
    {
        if (playerId <= 0)
        {
            yield break;
        }
        string url = $"{ApiClient.BaseUrl}/online-shop/sell/{playerId}";

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

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SimpleNotificationPopup.Show($"Bán thành công {quantity}x {itemType} với tổng giá {totalPrice}G!");
            
            bool itemRemoved = RemoveItemFromInventory(itemType, quantity);
            if (itemRemoved)
            {
                CollectableType parsedType = (CollectableType)System.Enum.Parse(typeof(CollectableType), collectableType);
                AddSoldItem(parsedType, quantity, totalPrice);
                
                if (player != null && player.inventory != null)
                {
                    player.inventory.NotifyInventoryChanged();
                }
            }
        }
        else
        {
            SimpleNotificationPopup.Show($"Bán thất bại! Lỗi: {request.error}");
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
    /// Làm mới danh sách (reset về slot rỗng, không destroy)
    /// </summary>
    private void ClearAllSlots()
    {
        Debug.Log($"🧹 [Sell Shop] Reset {soldSlots.Count} slot về trạng thái rỗng");
        
        foreach (var slot in soldSlots)
        {
            if (slot != null)
            {
                slot.SetupEmptySlot();
            }
        }
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
        string apiUrl = $"{ApiClient.BaseUrl}/online-shop/sold-items/{playerId}";
        
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            
            SoldItemsWrapper response = JsonUtility.FromJson<SoldItemsWrapper>(json);
            if (response != null && response.data != null)
            {
                DisplaySoldItemsOnUI(response.data);
            }
            else
            {
                ClearAllSlots();
            }
        }
        else
        {
            SimpleNotificationPopup.Show($"Không thể tải danh sách items! Lỗi: {request.error}");
            ClearAllSlots();
        }
    }

    private void DisplaySoldItemsOnUI(List<SellItemResponse> soldItems)
    {
        ClearAllSlots();
        EnsureEnoughSlots(soldItems.Count);
        
        for (int i = 0; i < soldItems.Count && i < soldSlots.Count; i++)
        {
            var item = soldItems[i];
            var slotUI = soldSlots[i];
            
            if (slotUI != null)
            {
                slotUI.Setup(item, this, OnClaimSingleItem);
            }
        }
    }

    /// <summary>
    /// Đảm bảo có đủ slot cho số lượng items
    /// </summary>
    private void EnsureEnoughSlots(int requiredSlots)
    {
        if (requiredSlots <= soldSlots.Count)
            return; // Đã có đủ slot

        int slotsToAdd = requiredSlots - soldSlots.Count;

        for (int i = 0; i < slotsToAdd; i++)
        {
            GameObject slotGO = Instantiate(soldItemSlotPrefab, sellSlotContainer);
            var slotUI = slotGO.GetComponent<SoldItemSlot_UI>();
            
            if (slotUI != null)
            {
                slotUI.SetupEmptySlot();
                soldSlots.Add(slotUI);
            }
            else
            {
                Destroy(slotGO);
            }
        }
    }

    private void OnClaimSingleItem(SellItemResponse item)
    {
        if (item.canBuy)
        {
            SimpleNotificationPopup.Show($"{item.collectableType} vẫn đang bán, chưa thể claim!");
            return;
        }
        
        List<int> singleItemId = new List<int> { item.id };
        StartCoroutine(ClaimMoneyCoroutine(singleItemId, item.price));
    }

    private IEnumerator ClaimMoneyCoroutine(List<int> itemIds, int expectedAmount)
    {
        string apiUrl = $"{ApiClient.BaseUrl}/online-shop/claim-money/{playerId}";
        string json = "[" + string.Join(",", itemIds) + "]";
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (player?.wallet != null)
            {
                player.wallet.Add(expectedAmount);
                Debug.Log($"💰 [Sell Shop] Đã cộng {expectedAmount}G vào wallet, tổng: {player.wallet.Money}G");
                SimpleNotificationPopup.Show($"Claim thành công {expectedAmount}G! Tổng tiền: {player.wallet.Money}G");
            }
            
            // Destroy specific slot sau khi claim thành công
            DestroyClaimedSlot(itemIds[0]); // Single item claim
            
            Debug.Log($"🎉 [Sell Shop] Claim thành công {expectedAmount}G và đã destroy slot!");
        }
        else
        {
            Debug.LogError($"❌ [Sell Shop] Claim money failed: {request.error}");
            SimpleNotificationPopup.Show($"Claim thất bại! Lỗi: {request.error}");
            
            // Re-enable slot button for failed items
            EnableSlotButtonsForItems(itemIds);
        }
    }

    /// <summary>
    /// Destroy specific slot sau khi claim thành công
    /// </summary>
    private void DestroyClaimedSlot(int itemId)
    {
        SoldItemSlot_UI slotToDestroy = null;
        foreach (var slot in soldSlots)
        {
            if (slot != null && slot.GetItemId() == itemId)
            {
                slotToDestroy = slot;
                break;
            }
        }
        
        if (slotToDestroy != null)
        {
            soldSlots.Remove(slotToDestroy);
            slotToDestroy.DestroySlot();
        }
    }

    private void EnableSlotButtonsForItems(List<int> itemIds)
    {
        foreach (var slot in soldSlots)
        {
            if (slot != null && itemIds.Contains(slot.GetItemId()))
            {
                slot.EnableSlotButton();
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
