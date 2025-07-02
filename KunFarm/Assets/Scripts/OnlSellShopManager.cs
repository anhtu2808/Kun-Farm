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

    [Header("UI")]
    public Transform sellSlotContainer;
    public GameObject shopSellSlotPrefab;

    private List<SoldItemSlot_UI> soldSlots = new();
    private bool isOpen = false;
    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (itemManager == null)
            itemManager = FindObjectOfType<ItemManager>();
        shopPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle shop bằng phím B chẳng hạn
        if (Input.GetKeyDown(KeyCode.O))
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

        isOpen = false;
        shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        isOpen = true;
        shopPanel.SetActive(true);
    }

    /// <summary>
    /// Gọi khi người chơi muốn bán item lên shop online
    /// </summary>
    public void SellItemOnline(CollectableType type, int quantity, int pricePerUnit)
    {
        Debug.Log($"🛒 [Online Sell] Bán item: {type}, Số lượng: {quantity}, Giá mỗi đơn vị: {pricePerUnit}G");
        int totalPrice = quantity * pricePerUnit;
        StartCoroutine(SendSellRequest(type.ToString(), quantity, totalPrice));

    }

    private IEnumerator SendSellRequest(string collectableType, int quantity, int totalPrice)
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
            CollectableType parsedType = (CollectableType)System.Enum.Parse(typeof(CollectableType), collectableType);
            AddSoldItem(parsedType, quantity, totalPrice);
            // TODO: Trừ item khỏi inventory nếu cần
        }
        else
        {
            Debug.LogError("❌ [Online Sell] Lỗi khi gửi request: " + request.error);
        }
        
    }

    private void AddSoldItem(CollectableType itemType, int quantity, int totalPrice)
    {
        Sprite icon = itemManager.GetItemByType(itemType).icon;
        System.DateTime now = System.DateTime.Now;

        GameObject newSlotGO = Instantiate(shopSellSlotPrefab, sellSlotContainer);
        SoldItemSlot_UI slotUI = newSlotGO.GetComponent<SoldItemSlot_UI>();

        if (slotUI != null)
        {
            slotUI.SetSoldItem(itemType, icon, quantity, totalPrice, now);
            slotUI.AnimateNewItem();
            soldSlots.Add(slotUI);
        }
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
}
