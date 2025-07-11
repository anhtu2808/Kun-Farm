using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

/// <summary>
/// Quản lý logic mua/bán items trong shop
/// </summary>
public class ShopManager : MonoBehaviour
{

    [Header("References")]
    public Player player;
    public GameObject shopPanel;
    public ItemManager itemManager;
    public Transform shopSlotContainer;
    public GameObject shopSlotPrefab;
    public PlayerInventoryScroll_UI playerInventoryScrollUI;
    // Events
    public System.Action OnShopUpdated;
    private bool isOpen = false;
    // API URL will be dynamically generated using ApiClient.BaseUrl
    private bool hasBuyItem = false;
    private List<BuyItemRequest> list;

    private void Awake()
    {
        // Tự động tìm references nếu chưa assign
        if (player == null)
            player = FindObjectOfType<Player>();

        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();

        if (playerInventoryScrollUI == null)
            playerInventoryScrollUI = FindObjectOfType<PlayerInventoryScroll_UI>(); // Nếu bạn chưa gán thủ công
        shopPanel.SetActive(false);
        int playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId > 0)
        {
            StartCoroutine(GetShopData(playerId));
        }
        else
        {
            Debug.LogWarning("[ShopManager] No valid player ID found, skipping shop data load");
        }
    }

    void Update()
    {
        // Input handling moved to UIManager  
        // B key is now handled by UIManager
    }

    public void ToggleShop()
    {
        Debug.Log($"Đóng shop - hasBuyItem: {hasBuyItem}");
        if (hasBuyItem)
        {
            StartCoroutine(SendBuyRequest(list));
        }

        hasBuyItem = false;
        list = null;
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

    private IEnumerator GetShopData(int playerId)
    {
        string url = $"{ApiClient.BaseUrl}/regular-shop/{playerId}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ShopResponseWrapper response = JsonUtility.FromJson<ShopResponseWrapper>(json);

            foreach (var item in response.data)
            {
                Debug.Log($"Loaded shop item: {item.collectableType} - {item.currentStock}/{item.stockLimit}");
                GameObject slotGO = Instantiate(shopSlotPrefab, shopSlotContainer);
                var slotUI = slotGO.GetComponent<ShopSlot_UI>();
                slotUI.Setup(item, this);
            }
        }
        else
        {
                Debug.Log("API lỗi: " + request.error);
        }
        // Removed refresh call from here - now done in OpenShop()
    }

    /// <summary>
    /// Mua item từ shop
    /// </summary>
    public void BuyItem(ShopSlotData data)
    {
        Debug.Log($"Mua item: {data.itemName} - Giá: {data.buyPrice} - Số lượng hiện tại: {data.currentStock}/{data.stockLimit}");
        
        // Kiểm tra hết hàng
        if (data.currentStock > data.stockLimit)
        {
            Debug.LogWarning("Đã hết hàng!");
            return;
        }

        // Kiểm tra đủ tiền
        if (player?.wallet == null)
        {
            Debug.LogError("❌ [Regular Shop] Player wallet không tìm thấy!");
            return;
        }

        if (player.wallet.Money < data.buyPrice)
        {
            Debug.LogWarning($"❌ [Regular Shop] Không đủ tiền! Cần: {data.buyPrice}G, Có: {player.wallet.Money}G");
            SimpleNotificationPopup.Show($"Không đủ tiền! Cần: {data.buyPrice}G, bạn có: {player.wallet.Money}G");
            return;
        }

        // Trừ tiền ngay lập tức
        bool moneySpent = player.wallet.Spend(data.buyPrice);
        if (!moneySpent)
        {
            Debug.LogError($"❌ [Regular Shop] Không thể trừ tiền! Cần: {data.buyPrice}G, Có: {player.wallet.Money}G");
            return;
        }

        Debug.Log($"💰 [Regular Shop] Đã trừ {data.buyPrice}G, còn lại: {player.wallet.Money}G");

        // Thêm vào batch request list để gửi API khi đóng shop
        if (list == null)
            list = new List<BuyItemRequest>();

        var existingItem = list.FirstOrDefault(x => x.SlotId == data.slotId);
        if (existingItem != null)
        {
            existingItem.Quantity += 1;
            existingItem.TotalPrice += data.buyPrice;
        }
        else
        {
            list.Add(new BuyItemRequest
            {
                SlotId = data.slotId,
                Quantity = 1,
                TotalPrice = data.buyPrice
            });
        }

        hasBuyItem = true;

        // Thêm item vào inventory sau khi đã trừ tiền thành công
        if (Enum.TryParse<CollectableType>(data.collectableType, ignoreCase: true, out var parsedType))
        {
            var collectable = itemManager.GetItemByType(parsedType);
            if (collectable != null)
            {
                player.inventory.Add(collectable, 1);
                player.inventory.NotifyInventoryChanged();
                Debug.Log($"✅ [Regular Shop] Đã thêm {data.itemName} vào inventory");
                SimpleNotificationPopup.Show($"Successfully purchased {data.itemName} for {data.buyPrice}G! Remaining balance: {player.wallet.Money}G");
            }
            else
            {
                Debug.LogError($"❌ [Regular Shop] Không tìm thấy collectable cho {parsedType}");
                // Hoàn tiền nếu không tìm thấy item
                player.wallet.Add(data.buyPrice);
                Debug.Log($"💰 [Regular Shop] Đã hoàn tiền {data.buyPrice}G do không tìm thấy item");
            }
        }
        else
        {
            Debug.LogError($"❌ [Regular Shop] Không parse được CollectableType từ '{data.collectableType}'");
            // Hoàn tiền nếu parse thất bại
            player.wallet.Add(data.buyPrice);
            Debug.Log($"💰 [Regular Shop] Đã hoàn tiền {data.buyPrice}G do lỗi parse");
        }
    }

    private IEnumerator SendBuyRequest(List<BuyItemRequest> requestList)
    {
        int playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId <= 0)
        {
            Debug.LogError("[ShopManager] No valid player ID for buy request");
            yield break;
        }
        string apiUrl = $"{ApiClient.BaseUrl}/regular-shop/buy/{playerId}";

        // ✅ Sử dụng class rõ ràng thay vì anonymous
        BuyItemRequestList wrapper = new BuyItemRequestList();
        wrapper.Items = requestList;

        string json = JsonUtility.ToJson(wrapper);
        Debug.Log("📤 Sending Buy Request: " + json); // kiểm tra JSON trước khi gửi

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Mua thành công! Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Mua thất bại: " + request.error);
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
}