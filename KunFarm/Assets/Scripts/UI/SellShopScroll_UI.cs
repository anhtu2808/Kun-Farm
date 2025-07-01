using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// UI Manager cho SellShop_Scroll - hiển thị lịch sử các vật phẩm đã được bán
/// </summary>
public class SellShopScroll_UI : MonoBehaviour
{
    [Header("References")]
    public ShopManager shopManager;
    public Player player;
    public Transform soldItemsContainer; // SellShop_Items container
    
    [Header("Item Slot Structure")]
    [Tooltip("Prefab structure: Item/icon(Image), Item/name(TextMeshProUGUI), Item/quantity(TextMeshProUGUI), Item/earnings(TextMeshProUGUI), Item/time(TextMeshProUGUI)")]
    public List<SoldItemSlot> soldItemSlots = new List<SoldItemSlot>();
    
    [Header("History Management")]
    public Button clearHistoryButton;
    public TextMeshProUGUI totalHistoryText;
    public int maxHistoryItems = 50; // Giới hạn số lượng items trong lịch sử
    
    [System.Serializable]
    public class SoldItemSlot
    {
        public GameObject itemObject;
        public Image iconImage;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI quantityText;
        public TextMeshProUGUI earningsText;
        public TextMeshProUGUI timeText;
        public SoldItemData soldItemData;
    }
    
    [System.Serializable]
    public class SoldItemData
    {
        public CollectableType itemType;
        public string itemName;
        public int quantity;
        public int earnings;
        public DateTime sellTime;
        public Sprite itemIcon;
        
        public SoldItemData(CollectableType type, string name, int qty, int earn, Sprite icon)
        {
            itemType = type;
            itemName = name;
            quantity = qty;
            earnings = earn;
            sellTime = DateTime.Now;
            itemIcon = icon;
        }
    }
    
    // Sold items history
    private List<SoldItemData> soldItemsHistory = new List<SoldItemData>();
    
    private void Awake()
    {
        // Auto-find references nếu chưa gán
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
            
        if (player == null)
            player = FindObjectOfType<Player>();
            
        // Tìm SellShop_Items container nếu chưa gán
        if (soldItemsContainer == null)
        {
            soldItemsContainer = transform.Find("SellShop_Items");
            if (soldItemsContainer == null)
            {
                Debug.LogError("SellShopScroll_UI: Không tìm thấy SellShop_Items container!");
                return;
            }
        }
        
        InitializeSoldItemSlots();
        SetupClearHistoryButton();
    }
    
    private void Start()
    {
        RefreshDisplay();
    }
    
    /// <summary>
    /// Initialize với references
    /// </summary>
    public void Initialize(ShopManager manager, Player playerRef)
    {
        shopManager = manager;
        player = playerRef;
        RefreshDisplay();
    }
    
    /// <summary>
    /// Khởi tạo các sold item slots từ children của SellShop_Items
    /// </summary>
    private void InitializeSoldItemSlots()
    {
        soldItemSlots.Clear();
        
        // Lấy tất cả children của SellShop_Items
        for (int i = 0; i < soldItemsContainer.childCount; i++)
        {
            Transform itemTransform = soldItemsContainer.GetChild(i);
            GameObject itemObject = itemTransform.gameObject;
            
            // Tìm components trong item
            Image iconImage = null;
            TextMeshProUGUI itemNameText = null;
            TextMeshProUGUI quantityText = null;
            TextMeshProUGUI earningsText = null;
            TextMeshProUGUI timeText = null;
            
            // Tìm icon
            iconImage = itemTransform.GetComponentInChildren<Image>();
            if (iconImage == itemObject.GetComponent<Image>()) // Skip background image
            {
                Image[] images = itemTransform.GetComponentsInChildren<Image>();
                iconImage = images.Length > 1 ? images[1] : images[0];
            }
            
            // Tìm text components
            TextMeshProUGUI[] texts = itemTransform.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI txt in texts)
            {
                if (txt.name.ToLower().Contains("name"))
                {
                    itemNameText = txt;
                }
                else if (txt.name.ToLower().Contains("quantity") || txt.name.ToLower().Contains("count"))
                {
                    quantityText = txt;
                }
                else if (txt.name.ToLower().Contains("earnings") || txt.name.ToLower().Contains("price"))
                {
                    earningsText = txt;
                }
                else if (txt.name.ToLower().Contains("time"))
                {
                    timeText = txt;
                }
            }
            
            // Tạo SoldItemSlot
            SoldItemSlot soldSlot = new SoldItemSlot
            {
                itemObject = itemObject,
                iconImage = iconImage,
                itemNameText = itemNameText,
                quantityText = quantityText,
                earningsText = earningsText,
                timeText = timeText
            };
            
            soldItemSlots.Add(soldSlot);
            
            // Initially hide item
            itemObject.SetActive(false);
        }
        
        Debug.Log($"SellShopScroll_UI: Initialized {soldItemSlots.Count} sold item slots");
    }
    
    /// <summary>
    /// Setup clear history button
    /// </summary>
    private void SetupClearHistoryButton()
    {
        if (clearHistoryButton != null)
        {
            clearHistoryButton.onClick.AddListener(ClearHistory);
        }
    }
    
    /// <summary>
    /// Thêm item đã bán vào lịch sử
    /// </summary>
    // public void AddSoldItem(CollectableType itemType, int quantity, int earnings)
    // {
    //     // Lấy thông tin item từ shop data
    //     // ShopItem shopItem = shopManager?.shopData?.GetShopItem(itemType);
    //     // string itemName = shopItem != null ? shopItem.itemName : itemType.ToString();
    //     // Sprite itemIcon = shopItem != null ? shopItem.itemIcon : null;
        
    //     // Tạo sold item data
    //     SoldItemData soldItem = new SoldItemData(itemType, itemName, quantity, earnings, itemIcon);
        
    //     // Thêm vào đầu danh sách (mới nhất lên đầu)
    //     soldItemsHistory.Insert(0, soldItem);
        
    //     // Giới hạn số lượng items trong lịch sử
    //     if (soldItemsHistory.Count > maxHistoryItems)
    //     {
    //         soldItemsHistory.RemoveAt(soldItemsHistory.Count - 1);
    //     }
        
    //     // Refresh display
    //     RefreshDisplay();
        
    //     Debug.Log($"SellShopScroll_UI: Added {quantity}x {itemName} to history for {earnings}G");
    // }
    
    /// <summary>
    /// Refresh display với sold items history
    /// </summary>
    public void RefreshDisplay()
    {
        // Cập nhật từng slot
        for (int i = 0; i < soldItemSlots.Count; i++)
        {
            if (i < soldItemsHistory.Count)
            {
                // Có item để hiển thị
                UpdateSoldItemSlot(i, soldItemsHistory[i]);
            }
            else
            {
                // Slot trống
                ClearSoldItemSlot(i);
            }
        }
        
        // Update total history info
        UpdateTotalHistoryInfo();
        
        Debug.Log($"SellShopScroll_UI: Refreshed display with {soldItemsHistory.Count} sold items");
    }
    
    /// <summary>
    /// Cập nhật một sold item slot
    /// </summary>
    private void UpdateSoldItemSlot(int slotIndex, SoldItemData soldItem)
    {
        if (slotIndex >= soldItemSlots.Count) return;
        
        SoldItemSlot slot = soldItemSlots[slotIndex];
        slot.soldItemData = soldItem;
        
        // Cập nhật icon
        if (slot.iconImage != null && soldItem.itemIcon != null)
        {
            slot.iconImage.sprite = soldItem.itemIcon;
            slot.iconImage.color = Color.white;
            slot.iconImage.gameObject.SetActive(true);
        }
        
        // Cập nhật item name
        if (slot.itemNameText != null)
        {
            slot.itemNameText.text = soldItem.itemName;
            slot.itemNameText.gameObject.SetActive(true);
        }
        
        // Cập nhật quantity
        if (slot.quantityText != null)
        {
            slot.quantityText.text = $"x{soldItem.quantity}";
            slot.quantityText.gameObject.SetActive(true);
        }
        
        // Cập nhật earnings
        if (slot.earningsText != null)
        {
            slot.earningsText.text = $"+{soldItem.earnings}G";
            slot.earningsText.color = Color.green;
            slot.earningsText.gameObject.SetActive(true);
        }
        
        // Cập nhật time
        if (slot.timeText != null)
        {
            TimeSpan timeSinceSold = DateTime.Now - soldItem.sellTime;
            string timeText = FormatTimeSinceSold(timeSinceSold);
            slot.timeText.text = timeText;
            slot.timeText.gameObject.SetActive(true);
        }
        
        // Show item object
        slot.itemObject.SetActive(true);
    }
    
    /// <summary>
    /// Clear một sold item slot
    /// </summary>
    private void ClearSoldItemSlot(int slotIndex)
    {
        if (slotIndex >= soldItemSlots.Count) return;
        
        SoldItemSlot slot = soldItemSlots[slotIndex];
        slot.soldItemData = null;
        
        // Hide all UI elements
        if (slot.iconImage != null)
            slot.iconImage.gameObject.SetActive(false);
        if (slot.itemNameText != null)
            slot.itemNameText.gameObject.SetActive(false);
        if (slot.quantityText != null)
            slot.quantityText.gameObject.SetActive(false);
        if (slot.earningsText != null)
            slot.earningsText.gameObject.SetActive(false);
        if (slot.timeText != null)
            slot.timeText.gameObject.SetActive(false);
            
        // Hide item object
        slot.itemObject.SetActive(false);
    }
    
    /// <summary>
    /// Format time since item was sold
    /// </summary>
    private string FormatTimeSinceSold(TimeSpan timeSpan)
    {
        if (timeSpan.TotalMinutes < 1)
        {
            return "Vừa xong";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        }
        else if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours} giờ trước";
        }
        else
        {
            return $"{(int)timeSpan.TotalDays} ngày trước";
        }
    }
    
    /// <summary>
    /// Update total history info
    /// </summary>
    private void UpdateTotalHistoryInfo()
    {
        if (totalHistoryText == null) return;
        
        int totalItems = soldItemsHistory.Count;
        int totalEarnings = 0;
        
        foreach (var soldItem in soldItemsHistory)
        {
            totalEarnings += soldItem.earnings;
        }
        
        totalHistoryText.text = $"Lịch sử: {totalItems} items - {totalEarnings}G";
    }
    
    /// <summary>
    /// Clear toàn bộ lịch sử
    /// </summary>
    public void ClearHistory()
    {
        soldItemsHistory.Clear();
        RefreshDisplay();
        
        Debug.Log("SellShopScroll_UI: History cleared");
    }
    
    /// <summary>
    /// Lấy thông tin sold item tại slot
    /// </summary>
    public SoldItemData GetSoldItemInfo(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < soldItemSlots.Count)
            return soldItemSlots[slotIndex].soldItemData;
        return null;
    }
    
    /// <summary>
    /// Lấy toàn bộ lịch sử bán hàng
    /// </summary>
    public List<SoldItemData> GetSoldItemsHistory()
    {
        return new List<SoldItemData>(soldItemsHistory);
    }
    
    /// <summary>
    /// Lấy tổng thu nhập từ lịch sử
    /// </summary>
    public int GetTotalEarnings()
    {
        int total = 0;
        foreach (var soldItem in soldItemsHistory)
        {
            total += soldItem.earnings;
        }
        return total;
    }
    
    /// <summary>
    /// Lấy tổng số items đã bán
    /// </summary>
    public int GetTotalItemsSold()
    {
        int total = 0;
        foreach (var soldItem in soldItemsHistory)
        {
            total += soldItem.quantity;
        }
        return total;
    }
    
    /// <summary>
    /// Export lịch sử bán hàng (có thể dùng để save/load)
    /// </summary>
    public string ExportHistory()
    {
        // Simple JSON-like format
        string history = "";
        foreach (var soldItem in soldItemsHistory)
        {
            history += $"{soldItem.itemType},{soldItem.itemName},{soldItem.quantity},{soldItem.earnings},{soldItem.sellTime:yyyy-MM-dd HH:mm:ss};";
        }
        return history;
    }
    
    /// <summary>
    /// Import lịch sử bán hàng (có thể dùng để save/load)
    /// </summary>
    public void ImportHistory(string historyData)
    {
        if (string.IsNullOrEmpty(historyData)) return;
        
        soldItemsHistory.Clear();
        
        string[] items = historyData.Split(';');
        foreach (string item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;
            
            string[] parts = item.Split(',');
            if (parts.Length >= 5)
            {
                try
                {
                    CollectableType itemType = (CollectableType)System.Enum.Parse(typeof(CollectableType), parts[0]);
                    string itemName = parts[1];
                    int quantity = int.Parse(parts[2]);
                    int earnings = int.Parse(parts[3]);
                    DateTime sellTime = DateTime.Parse(parts[4]);
                    
                    SoldItemData soldItem = new SoldItemData(itemType, itemName, quantity, earnings, null);
                    soldItem.sellTime = sellTime;
                    
                    soldItemsHistory.Add(soldItem);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"SellShopScroll_UI: Failed to parse history item: {item} - {e.Message}");
                }
            }
        }
        
        RefreshDisplay();
        Debug.Log($"SellShopScroll_UI: Imported {soldItemsHistory.Count} history items");
    }
} 