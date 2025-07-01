using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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
    // Events
    public System.Action OnShopUpdated;
    private bool isOpen = false;
    private string apiUrl = "https://localhost:7067/regular-shop/{playerId}";
    private bool hasBuyItem = false;
    private void Awake()
    {
        // Tự động tìm references nếu chưa assign
        if (player == null)
            player = FindObjectOfType<Player>();

        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();
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
    }

    /// <summary>
    /// Mua item từ shop
    /// </summary>
    public void BuyItem(ShopSlotData data)
    {
        if (data.currentStock >= data.stockLimit)
        {
            Debug.LogWarning("Đã hết hàng!");
            return;
        }
        hasBuyItem = true;
        // Gọi tới Wallet để trừ tiền + Inventory để thêm item (nếu đủ)
        Debug.Log($"Mua: {data.itemName} với giá {data.buyPrice}");
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