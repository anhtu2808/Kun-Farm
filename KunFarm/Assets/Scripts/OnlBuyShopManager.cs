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
    // Events
    public System.Action OnShopUpdated;
    private bool isOpen = false;

    private bool hasBuyItem = false;
    private List<int> list;

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
        StartCoroutine(GetShopData());
    }

    void Update()
    {
        // Input handling moved to UIManager
        // P key is now handled by UIManager
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
    }

    private IEnumerator GetShopData(int playerId = 2)
    {
        string apiUrl = "http://localhost:5270/online-shop/{playerId}";
        string url = apiUrl.Replace("{playerId}", playerId.ToString());

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            BuyShopWrapper response = JsonUtility.FromJson<BuyShopWrapper>(json);
            foreach (var item in response.data)
            {

                Debug.Log($"[Get Online Shop Data]] Loaded shop item: {item.collectableType} - {item.price} - {item.canBuy} - {item.quantity}");
                GameObject slotGO = Instantiate(shopSlotPrefab, shopSlotContainer);
                var slotUI = slotGO.GetComponent<ShopBuySlot_UI>();
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
    public void BuyItem(SellItemResponse data)
    {

        if (list == null)
            list = new List<int>();

        list.Add(data.id);

        hasBuyItem = true;

        if (Enum.TryParse<CollectableType>(data.collectableType, ignoreCase: true, out var parsedType))
        {
            var collectable = itemManager.GetItemByType(parsedType);
            if (collectable != null)
            {
                player.inventory.Add(collectable, data.quantity);
                player.inventory.NotifyInventoryChanged();
            }
        }
        else
        {
            Debug.LogError($"Không parse được CollectableType từ '{data.collectableType}'");
        }
        // Gọi tới Wallet để trừ tiền + Inventory để thêm item (nếu đủ)
    }

    private IEnumerator SendBuyRequest(List<int> requestList)
    {
        string apiUrl = "http://localhost:5270/online-shop/buy/1";

        string json = "[" + string.Join(",", requestList) + "]";
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

[Serializable]
public class IntListWrapper
{
    public List<int> items;
}
