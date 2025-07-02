using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// UI Manager cho Player_Scroll - hiển thị inventory items với sell functionality
/// </summary>
public class PlayerInventoryScroll_UI : MonoBehaviour
{
    [Header("References")]
    //     public ShopManager shopManager;
    public Player player;
    public Transform playerItemsContainer; // Player_Items container
    public GameObject itemPrefab;
    //     [Header("Item Slot Structure")]
    //     [Tooltip("Prefab structure: Item/icon(Image), Item/buybutton(Button), Item/price(TextMeshProUGUI), Item/quantity(TextMeshProUGUI)")]
    //     public List<PlayerScrollItem> itemSlots = new List<PlayerScrollItem>();

    [System.Serializable]
    public class PlayerScrollItem
    {
        public GameObject itemObject;
        public Image iconImage;
        public TextMeshProUGUI priceText;

        public TextMeshProUGUI quantityText;
        public CollectableType currentItemType = CollectableType.NONE;
    }
    private List<PlayerScrollItem> itemSlots = new List<PlayerScrollItem>();

    private void Awake()
    {
        // Auto-find references nếu chưa gán
        // if (shopManager == null)
        //     shopManager = FindObjectOfType<ShopManager>();

        if (player == null)
            player = FindObjectOfType<Player>();

        // Tìm Player_Items container nếu chưa gán
        if (playerItemsContainer == null)
        {
            playerItemsContainer = transform.Find("Player_Items");
            if (playerItemsContainer == null)
            {
                Debug.LogError("PlayerInventoryScroll_UI: Không tìm thấy Player_Items container!");
                return;
            }
        }

        // InitializeItemSlots();
    }

    private void Start()
    {
        // Initial setup - OnEnable will handle the subscription
    }
    
    private void OnEnable()
    {
        // Đăng ký event listener khi component được kích hoạt
        if (player != null && player.inventory != null)
        {
            player.inventory.onInventoryChanged -= RefreshInventoryUI; // Remove to avoid duplicate
            player.inventory.onInventoryChanged += RefreshInventoryUI;
            
            // Gọi refresh trực tiếp thay vì qua coroutine để tránh lỗi inactive object
            RefreshInventoryUI();
        }
    }
    
    private void OnDisable()
    {
        // Clean up event listener khi component bị vô hiệu hóa
        if (player != null && player.inventory != null)
        {
            player.inventory.onInventoryChanged -= RefreshInventoryUI;
        }
    }

    public void RefreshInventoryUI()
    {
        // Check if we're in a valid state
        if (playerItemsContainer == null)
        {
            Debug.LogWarning("PlayerInventoryScroll_UI: playerItemsContainer is null, cannot refresh");
            return;
        }

        foreach (Transform child in playerItemsContainer)
        {
            Destroy(child.gameObject);
        }

        itemSlots.Clear();
        if (player == null || player.inventory == null)
        {
            Debug.LogWarning("PlayerInventoryScroll_UI: Player or inventory is not set!");
            return;
        }
        
        foreach (var slot in player.inventory.slots)
        {
            if (slot.type == CollectableType.NONE || slot.count <= 0)
                continue;

            GameObject go = Instantiate(itemPrefab, playerItemsContainer);
            PlayerScrollItem scrollItem = new PlayerScrollItem
            {
                itemObject = go,
                iconImage = go.transform.Find("icon").GetComponent<Image>(),
                quantityText = go.transform.Find("quantity").GetComponent<TextMeshProUGUI>(),
                currentItemType = slot.type
            };

            scrollItem.iconImage.sprite = slot.icon;
            scrollItem.quantityText.text = slot.count.ToString();

            itemSlots.Add(scrollItem);
        }
        
        Debug.Log($"PlayerInventoryScroll_UI: Refreshed with {itemSlots.Count} items");
    }
    
    /// <summary>
    /// Force refresh với delay - sử dụng khi có timing issues
    /// </summary>
    public void ForceRefreshWithDelay()
    {
        // Kiểm tra nếu GameObject đang active trước khi start coroutine
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ForceRefreshCoroutine());
        }
        else
        {
            // Nếu object inactive, gọi refresh trực tiếp khi có thể
            Debug.LogWarning("PlayerInventoryScroll_UI: GameObject inactive, calling direct refresh");
            RefreshInventoryUI();
        }
    }
    
    private System.Collections.IEnumerator ForceRefreshCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // Double frame wait for safety
        RefreshInventoryUI();
    }

}

//     private void Start()
//     {
//         RefreshDisplay();
//     }

//     /// <summary>
//     /// Khởi tạo các item slots từ children của Player_Items
//     /// </summary>
//     private void InitializeItemSlots()
//     {
//         itemSlots.Clear();

//         // Lấy tất cả children của Player_Items
//         for (int i = 0; i < playerItemsContainer.childCount; i++)
//         {
//             Transform itemTransform = playerItemsContainer.GetChild(i);
//             GameObject itemObject = itemTransform.gameObject;

//             // Tìm components trong item
//             Image iconImage = null;
//             Button sellButton = null;
//             TextMeshProUGUI priceText = null;
//             TextMeshProUGUI quantityText = null;
//             // Tìm icon (có thể có nested structure)
//             iconImage = itemTransform.GetComponentInChildren<Image>();
//             if (iconImage == itemObject.GetComponent<Image>()) // Skip background image
//             {
//                 Image[] images = itemTransform.GetComponentsInChildren<Image>();
//                 iconImage = images.Length > 1 ? images[1] : images[0];
//             }

//             // Tìm buybutton (sell button)
//             Button[] buttons = itemTransform.GetComponentsInChildren<Button>();
//             foreach (Button btn in buttons)
//             {
//                 if (btn.name.ToLower().Contains("buy"))
//                 {
//                     sellButton = btn;
//                     break;
//                 }
//             }

//             // Tìm price text
//             TextMeshProUGUI[] texts = itemTransform.GetComponentsInChildren<TextMeshProUGUI>();
//             foreach (TextMeshProUGUI txt in texts)
//             {
//                 if (txt.name.ToLower().Contains("price"))
//                 {
//                     priceText = txt;
//                 } else if (txt.name.ToLower().Contains("quantity") || txt.name.ToLower().Contains("count"))
//                 {
//                     quantityText = txt;
//                 }
//             }

//             // Tạo PlayerScrollItem
//             PlayerScrollItem scrollItem = new PlayerScrollItem
//             {
//                 itemObject = itemObject,
//                 iconImage = iconImage,
//                 sellButton = sellButton,
//                 priceText = priceText,
//                 quantityText = quantityText
//             };

//             itemSlots.Add(scrollItem);

//             // Setup sell button event
//             if (sellButton != null)
//             {
//                 int slotIndex = i;
//                 sellButton.onClick.RemoveAllListeners();
//                 sellButton.onClick.AddListener(() => OnSellButtonClicked(slotIndex));
//             }

//             Debug.Log($"PlayerInventoryScroll_UI: Initialized slot {i} - Icon: {(iconImage != null)}, Button: {(sellButton != null)}, Price: {(priceText != null)}");
//         }

//         Debug.Log($"PlayerInventoryScroll_UI: Initialized {itemSlots.Count} item slots");
//     }

/// <summary>
/// Refresh hiển thị tất cả items
/// </summary>
// public void RefreshDisplay()
// {
//     if (player == null || player.inventory == null)
//     {
//         Debug.LogWarning("PlayerInventoryScroll_UI: Missing references for refresh");
//         return;
//     }

//     // Lấy danh sách items từ inventory
//     List<Inventory.Slot> inventoryItems = new List<Inventory.Slot>();
//     foreach (var slot in player.inventory.slots)
//     {
//         if (slot.type != CollectableType.NONE && slot.count > 0)
//         {
//             inventoryItems.Add(slot);
//         }
//     }

// // Cập nhật từng slot
// for (int i = 0; i < itemSlots.Count; i++)
// {
//     if (i < inventoryItems.Count)
//     {
//         // Có item để hiển thị
//         UpdateSlot(i, inventoryItems[i]);
//     }
//     else
//     {
//         // Slot trống
//         ClearSlot(i);
//     }
// }

//     Debug.Log($"PlayerInventoryScroll_UI: Refreshed display with {inventoryItems.Count} items");
// }

//     /// <summary>
//     /// Cập nhật một slot với item data
//     /// </summary>
// private void UpdateSlot(int slotIndex, Inventory.Slot inventorySlot)
// {
//     if (slotIndex >= itemSlots.Count) return;

//     PlayerScrollItem scrollItem = itemSlots[slotIndex];
//     scrollItem.currentItemType = inventorySlot.type;
//     scrollItem.currentItemCount = inventorySlot.count;

//     // Lấy shop item data
//     ShopItem shopItem = shopManager.shopData.GetShopItem(inventorySlot.type);

//     // Cập nhật icon
//     if (scrollItem.iconImage != null && shopItem != null)
//     {
//         scrollItem.iconImage.sprite = shopItem.itemIcon;
//         scrollItem.iconImage.color = Color.white;
//         scrollItem.iconImage.gameObject.SetActive(true);
//     }

//     // Cập nhật price text
//     if (scrollItem.priceText != null)
//     {
//         if (shopItem != null && shopItem.canSell)
//         {
//             int totalValue = shopItem.sellPrice * inventorySlot.count;
//             scrollItem.priceText.text = $"{totalValue}G";
//             scrollItem.priceText.color = Color.yellow;
//         }
//         else
//         {
//             scrollItem.priceText.text = "Không bán được";
//             scrollItem.priceText.color = Color.red;
//         }
//         scrollItem.priceText.gameObject.SetActive(true);
//     }

//     if (scrollItem.quantityText != null)
//     {
//         scrollItem.quantityText.text = $"x{inventorySlot.count}";
//         scrollItem.quantityText.gameObject.SetActive(true);
//     }

//     // Show item object
//     scrollItem.itemObject.SetActive(true);
// }

//     /// <summary>
//     /// Xóa hiển thị của một slot
//     /// </summary>
//     private void ClearSlot(int slotIndex)
//     {
//         if (slotIndex >= itemSlots.Count) return;

//         PlayerScrollItem scrollItem = itemSlots[slotIndex];
//         scrollItem.currentItemType = CollectableType.NONE;
//         scrollItem.currentItemCount = 0;

//         // Ẩn icon
//         if (scrollItem.iconImage != null)
//         {
//             scrollItem.iconImage.sprite = null;
//             scrollItem.iconImage.gameObject.SetActive(false);
//         }

//         // Ẩn price text
//         if (scrollItem.priceText != null)
//         {
//             scrollItem.priceText.gameObject.SetActive(false);
//         }

//         // Ẩn sell button
//         if (scrollItem.sellButton != null)
//         {
//             scrollItem.sellButton.gameObject.SetActive(false);
//         }

//         if (scrollItem.quantityText != null)
//             scrollItem.quantityText.gameObject.SetActive(false);

//         // Hide item object
//         scrollItem.itemObject.SetActive(false);
//     }

//     /// <summary>
//     /// Xử lý khi click sell button
//     /// </summary>
//     private void OnSellButtonClicked(int slotIndex)
//     {
//         if (slotIndex >= itemSlots.Count) return;

//         PlayerScrollItem scrollItem = itemSlots[slotIndex];

//         if (scrollItem.currentItemType == CollectableType.NONE || scrollItem.currentItemCount <= 0)
//         {
//             Debug.LogWarning("PlayerInventoryScroll_UI: No item to sell in slot " + slotIndex);
//             return;
//         }

//         // Find the actual inventory slot index for this item type
//         int inventorySlotIndex = FindInventorySlotIndex(scrollItem.currentItemType);
//         if (inventorySlotIndex == -1)
//         {
//             Debug.LogWarning($"PlayerInventoryScroll_UI: Could not find inventory slot for {scrollItem.currentItemType}");
//             return;
//         }

//         // Bán toàn bộ stack
//         bool success = shopManager.SellItem(inventorySlotIndex, scrollItem.currentItemCount);

//         if (success)
//         {
//             ShopItem shopItem = shopManager.shopData.GetShopItem(scrollItem.currentItemType);
//             int totalEarned = shopItem.sellPrice * scrollItem.currentItemCount;

//             Debug.Log($"PlayerInventoryScroll_UI: Sold {scrollItem.currentItemCount}x {scrollItem.currentItemType} for {totalEarned}G");

//             // Refresh display sau khi bán
//             RefreshDisplay();
//         }
//         else
//         {
//             Debug.LogWarning($"PlayerInventoryScroll_UI: Failed to sell {scrollItem.currentItemType}");
//         }
//     }

//     /// <summary>
//     /// Lấy thông tin item tại slot
//     /// </summary>
//     public PlayerScrollItem GetSlotInfo(int slotIndex)
//     {
//         if (slotIndex >= 0 && slotIndex < itemSlots.Count)
//             return itemSlots[slotIndex];
//         return null;
//     }

//     /// <summary>
//     /// Bán tất cả items có thể bán
//     /// </summary>
//     public void SellAllItems()
//     {
//         if (player == null || player.inventory == null) return;

//         int totalSold = 0;
//         float totalEarned = 0f;

//         // Bán tất cả items - process from back to front to avoid index shifting
//         for (int i = player.inventory.slots.Count - 1; i >= 0; i--)
//         {
//             var slot = player.inventory.slots[i];
//             if (slot.type != CollectableType.NONE && slot.count > 0)
//             {
//                 ShopItem shopItem = shopManager?.shopData?.GetShopItem(slot.type);
//                 if (shopItem != null && shopItem.canSell)
//                 {
//                     if (shopManager.SellItem(i, slot.count))
//                     {
//                         totalSold += slot.count;
//                         totalEarned += shopItem.sellPrice * slot.count;
//                     }
//                 }
//             }
//         }

//         if (totalSold > 0)
//         {
//             Debug.Log($"PlayerInventoryScroll_UI: Sold {totalSold} items for {totalEarned}G total");
//             RefreshDisplay();
//         }
//         else
//         {
//             Debug.Log("PlayerInventoryScroll_UI: No items were sold");
//         }
//     }

//     /// <summary>
//     /// Auto-refresh khi inventory thay đổi
//     /// </summary>
//     private void Update()
//     {
//         // Refresh mỗi 60 frames
//         if (Time.frameCount % 60 == 0)
//         {
//             RefreshDisplay();
//         }
//     }

//     /// <summary>
//     /// Find inventory slot index for a specific item type
//     /// </summary>
//     private int FindInventorySlotIndex(CollectableType itemType)
//     {
//         if (player == null || player.inventory == null) return -1;

//         for (int i = 0; i < player.inventory.slots.Count; i++)
//         {
//             var slot = player.inventory.slots[i];
//             if (slot.type == itemType && slot.count > 0)
//             {
//                 return i;
//             }
//         }
//         return -1;
//     }