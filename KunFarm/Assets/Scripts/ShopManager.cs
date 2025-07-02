using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    private string apiUrl = "https://localhost:7067/regular-shop/{playerId}";
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
        StartCoroutine(GetShopData(1));
    }

    void Update()
    {
        // Toggle shop bằng phím B chẳng hạn
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
    }

    public void CloseShop()
    {
        if (hasBuyItem)
        {
            StartCoroutine(SendBuyRequest(list));
        }

        hasBuyItem = false;
        isOpen = false;
        shopPanel.SetActive(false);

    }

    public void OpenShop()
    {
        isOpen = true;
        shopPanel.SetActive(true);
    }

    private IEnumerator GetShopData(int playerId)
    {
        string url = apiUrl.Replace("{playerId}", playerId.ToString());

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
            Debug.LogError("API lỗi: " + request.error);
        }
        playerInventoryScrollUI.RefreshInventoryUI();
    }

    /// <summary>
    /// Mua item từ shop
    /// </summary>
    public void BuyItem(ShopSlotData data)
    {
        Debug.Log($"Mua item: {data.itemName} - Giá: {data.buyPrice} - Số lượng hiện tại: {data.currentStock}/{data.stockLimit}");
        if (data.currentStock >= data.stockLimit)
        {
            Debug.LogWarning("Đã hết hàng!");
            return;
        }

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
        // Gọi tới Wallet để trừ tiền + Inventory để thêm item (nếu đủ)
        Debug.Log($"Mua: {data.itemName} với giá {data.buyPrice}");
    }

    private IEnumerator SendBuyRequest(List<BuyItemRequest> requestList)
    {
        string apiUrl = "https://localhost:7067/regular-shop/buy/1"; // ví dụ: /shop/buy/{playerId}

        var wrapper = new { items = requestList };
        string json = JsonUtility.ToJson(wrapper); // HOẶC dùng Newtonsoft.Json nếu cần

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("📤 Sending Buy Request: " + json);
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