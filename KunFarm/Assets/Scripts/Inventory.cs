using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class Inventory : MonoBehaviour
{
    public Transform inventorySlotContainer;
    public GameObject inventorySlotPrefab;
    public Player player;
    public GameObject inventoryPanel; // ← Gán panel chứa UI inventory
    public bool IsOpen => inventoryPanel.activeSelf;
    private int totalSlots = 27;
    private List<GameObject> slotInstances = new();
    
    // Track changes for auto-save
    private bool hasChanges = false;
    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 30f; // 30 seconds
    private bool isAutoSaveEnabled = true;

    [System.Serializable]
    public class Slot
    {
        public CollectableType type;
        public int count;
        public int maxAllowed;
        public Sprite icon;

        public Slot()
        {
            type = CollectableType.NONE;
            count = 0;
            maxAllowed = 99;
        }

        public bool CanAddItem()
        {
            if (count < maxAllowed) return true;

            return false;
        }

        public void AddItem(Collectable item, int quantity)
        {
            this.type = item.type;
            this.icon = item.icon;
            count += quantity;
        }

        public void RemoveItem()
        {
            if (count > 0)
            {
                count--;
                if (count == 0)
                {
                    icon = null;
                    type = CollectableType.NONE;
                }
            }
        }

    }

    // public Inventory(int numSlots)
    // {
    //     for (int i = 0; i < numSlots; i++)
    //     {
    //         Slot slot = new();
    //         slots.Add(slot);
    //     }
    // }

    private void Awake()
    {
        inventoryPanel.SetActive(false);
        InitializeEmptySlots();
        int playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId > 0)
        {
            StartCoroutine(GetInventoryData(playerId));
        }
        else
        {
            Debug.LogWarning("[Inventory] No valid player ID found, skipping inventory load");
        }
    }

    void Update()
    {
        // Input handling moved to UIManager
        // Tab key is now handled by UIManager
        
        // Auto save every 30 seconds
        if (isAutoSaveEnabled)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                autoSaveTimer = 0f;
                if (hasChanges)
                {
                    Debug.Log($"🕐 [Auto Save] Timer triggered, saving all 27 slots");
                    StartCoroutine(SaveAllSlots());
                }
            }
        }
    }

    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            Refresh();
        }
        else
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < slotInstances.Count; i++)
        {
            var uiSlot = slotInstances[i].GetComponent<Slot_UI>();
            if (i < slots.Count && slots[i].type != CollectableType.NONE)
            {
                uiSlot.SetItem(slots[i]); // Truyền Slot, hoặc tạo SlotData DTO
            }
            else
            {
                uiSlot.SetEmpty();
            }
        }
    }

    /// <summary>
    /// Khởi tạo 27 slot trống ban đầu
    /// </summary>
    private void InitializeEmptySlots()
    {
        Debug.Log("🔄 [Inventory] Khởi tạo 27 slot trống...");
        
        // Tạo 27 slot GameObject
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventorySlotContainer);
            slotInstances.Add(slotGO);
            
            // Setup UI slot trống
            Slot_UI slotUI = slotGO.GetComponent<Slot_UI>();
            InventorySlotData emptyData = new InventorySlotData
            {
                id = -1,
                slotIndex = i,
                itemId = 0,
                collectableType = "NONE",
                icon = "NONE",
                quantity = 0
            };
            slotUI.Setup(emptyData);
            StartCoroutine(DelayedInitDragDrop(slotUI, i));
            
            // Tạo slot data trống
            var emptySlot = new Slot
            {
                type = CollectableType.NONE,
                icon = null,
                count = 0,
                maxAllowed = 99
            };
            slots.Add(emptySlot);
        }
        
        Debug.Log("✅ [Inventory] Đã khởi tạo 27 slot trống");
    }

    private IEnumerator GetInventoryData(int playerId = 0)
    {
        Debug.Log("📡 [Inventory] Đang gọi API để load data...");
        string apiUrl = "http://localhost:5270/inventory/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(apiUrl.Replace("{playerId}", playerId.ToString()));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ [Inventory] API thành công, đang update slots...");
            string json = request.downloadHandler.text;
            InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(json);

            // Update slots với data từ API
            foreach (var apiSlot in response.data)
            {
                if (apiSlot.slotIndex >= 0 && apiSlot.slotIndex < totalSlots)
                {
                    // Update slot data
                    var slot = slots[apiSlot.slotIndex];
                    slot.type = System.Enum.TryParse<CollectableType>(apiSlot.collectableType, out var parsedType) ? parsedType : CollectableType.NONE;
                    slot.icon = Resources.Load<Sprite>($"Sprites/{apiSlot.icon}");
                    slot.count = apiSlot.quantity;
                    
                    // Update UI slot
                    Slot_UI slotUI = slotInstances[apiSlot.slotIndex].GetComponent<Slot_UI>();
                    slotUI.Setup(apiSlot);
                    
                    Debug.Log($"📦 [Inventory] Updated slot {apiSlot.slotIndex}: {apiSlot.collectableType} x{apiSlot.quantity}");
                }
            }
            
            Debug.Log($"✅ [Inventory] Đã load {response.data.Count} items từ API");
        }
        else
        {
            Debug.LogWarning($"⚠️ [Inventory] API lỗi: {request.error}");
            Debug.Log("💡 [Inventory] Sử dụng 27 slot trống như đã init");
            // Không làm gì cả, giữ nguyên 27 slot trống đã init
        }
    }

    private IEnumerator DelayedInitDragDrop(Slot_UI slotUI, int index)
    {
        yield return null; // đợi 1 frame để Awake() xong
        slotUI.InitializeDragDrop(SlotType.Inventory, index);
    }

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    public List<Slot> slots = new List<Slot>();
    public void Add(Collectable item, int quantity)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].type == item.type && slots[i].CanAddItem())
            {
                slots[i].AddItem(item, quantity);
                MarkInventoryChanged();
                onInventoryChanged?.Invoke();
                return;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].type == CollectableType.NONE)
            {
                slots[i].AddItem(item, quantity);
                MarkInventoryChanged();
                onInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void Remove(int index)
    {
        slots[index].RemoveItem();
        MarkInventoryChanged();
        var type = player.inventory.slots[index].type;
        var itemToDrop = GameManager.instance.itemManager.GetItemByType(type);

        if (itemToDrop == null)
        {
            Debug.LogWarning("Không tìm thấy item tương ứng để drop!");
            return;
        }

        player.DropItem(itemToDrop);
        player.inventory.Remove(index);
        Refresh();
    }

    public void AddItemByType(CollectableType type, Sprite icon, int count = 1)
    {
        // Find existing slot with same type
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].type == type && slots[i].CanAddItem())
            {
                slots[i].type = type;
                slots[i].icon = icon;
                slots[i].count += count;
                MarkInventoryChanged();
                onInventoryChanged?.Invoke();
                return;
            }
        }

        // Find empty slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].type == CollectableType.NONE)
            {
                slots[i].type = type;
                slots[i].icon = icon;
                slots[i].count = count;
                MarkInventoryChanged();
                onInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // Method to clear entire slot and trigger event
    public void ClearSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            Debug.Log($"Clearing slot {index} - {slots[index].type}");
            slots[index].count = 0;
            slots[index].type = CollectableType.NONE;
            slots[index].icon = null;
            MarkInventoryChanged();
            onInventoryChanged?.Invoke();
        }
    }

    // Method to trigger inventory changed event from external scripts
    public void NotifyInventoryChanged()
    {
        onInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Mark inventory as changed for auto-save
    /// </summary>
    public void MarkInventoryChanged()
    {
        hasChanges = true;
        Debug.Log($"📝 [Change Tracking] Inventory marked as changed");
    }
    
    /// <summary>
    /// Force save all slots immediately (public method)
    /// </summary>
    public void ForceSaveChanges()
    {
        if (hasChanges)
        {
            Debug.Log($"🚨 [Force Save] Manually triggered, saving all 27 slots");
            StartCoroutine(SaveAllSlots());
        }
        else
        {
            Debug.Log("💡 [Force Save] No changes to save");
        }
    }
    
        /// <summary>
    /// Save all 27 slots to server
    /// </summary>
    private IEnumerator SaveAllSlots()
    {
        if (!hasChanges) yield break;
        
        Debug.Log($"💾 [Save All] Saving all 27 slots...");
        
        // Prepare all 27 slots data
        UpdateInventorySlotRequest[] allSlots = new UpdateInventorySlotRequest[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            allSlots[i] = new UpdateInventorySlotRequest
            {
                slotIndex = i,
                collectableType = slots[i].type.ToString(),
                quantity = slots[i].count
            };
            Debug.Log($"Slot {i}: Type={allSlots[i].collectableType}, Quantity={allSlots[i].quantity}");
        }
        
        BatchUpdateInventoryRequest batchRequest = new BatchUpdateInventoryRequest
        {
            slots = allSlots
        };
        
        yield return StartCoroutine(SendBatchUpdateRequest(batchRequest));

        
        Debug.Log($"✅ [Save All] Completed saving all 27 slots");
    }
    
    /// <summary>
    /// Send batch update request to server
    /// </summary>
    private IEnumerator SendBatchUpdateRequest(BatchUpdateInventoryRequest request)
    {
        int playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId <= 0)
        {
            Debug.LogError("[Inventory] No valid player ID for batch update");
            yield break;
        }
        string apiUrl = $"http://localhost:5270/inventory/batch-update/{playerId}";
        string json = JsonUtility.ToJson(request);
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "PUT");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        
        Debug.Log($"📤 [Batch Update] Sending all 27 slots...");
        
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ [Batch Update] All slots updated successfully");
            hasChanges = false; // Clear changes flag
        }
        else
        {
            Debug.LogError($"❌ [Batch Update] Failed to update slots: {webRequest.error}");
            // Keep hasChanges = true for retry
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("▶️ [Quit] App đang thoát... Lưu dữ liệu!");

        // Force save all slots if there are changes
        if (hasChanges)
        {
            Debug.Log($"🚨 [Quit] Force saving all 27 slots...");
            StartCoroutine(SaveAllSlotsSync());
        }
        else
        {
            Debug.Log("💡 [Quit] No changes to save");
        }
    }
    
    /// <summary>
    /// Synchronous save for application quit
    /// </summary>
    private IEnumerator SaveAllSlotsSync()
    {
        isAutoSaveEnabled = false; // Disable auto save during quit
        
        // Prepare all 27 slots data
        UpdateInventorySlotRequest[] allSlots = new UpdateInventorySlotRequest[totalSlots];
        
        for (int i = 0; i < totalSlots; i++)
        {
            allSlots[i] = new UpdateInventorySlotRequest
            {
                slotIndex = i,
                collectableType = slots[i].type.ToString(),
                quantity = slots[i].count
            };
        }
        
        BatchUpdateInventoryRequest batchRequest = new BatchUpdateInventoryRequest
        {
            slots = allSlots
        };
        
        yield return StartCoroutine(SendBatchUpdateRequestSync(batchRequest));
        
        Debug.Log($"✅ [Quit] Completed saving all 27 slots before quit");
    }
    
    /// <summary>
    /// Synchronous version for quit save
    /// </summary>
    private IEnumerator SendBatchUpdateRequestSync(BatchUpdateInventoryRequest request)
    {
        int playerId = PlayerPrefs.GetInt("PLAYER_ID", 0);
        if (playerId <= 0)
        {
            Debug.LogError("[Inventory] No valid player ID for quit save");
            yield break;
        }
        string apiUrl = $"http://localhost:5270/inventory/batch-update/{playerId}";
        string json = JsonUtility.ToJson(request);
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "PUT");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        
        Debug.Log($"📤 [Quit Save] Sending all 27 slots...");
        
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ [Quit Save] All slots saved successfully");
            hasChanges = false;
        }
        else
        {
            Debug.LogError($"❌ [Quit Save] Failed to save slots: {webRequest.error}");
        }
    }



}
