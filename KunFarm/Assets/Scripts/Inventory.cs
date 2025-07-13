using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Transform inventorySlotContainer;
    public GameObject inventorySlotPrefab;

    public GameObject closeButtonPrefab;
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
        
        // Setup close button
        SetupCloseButton();
        
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

    /// <summary>
    /// Closes the inventory panel. This method is specifically for button clicks.
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Opens the inventory panel. This method is specifically for button clicks.
    /// </summary>
    public void OpenInventory()
    {
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            Refresh();
        }
    }

    /// <summary>
    /// Sets up the close button click event
    /// </summary>
    private void SetupCloseButton()
    {
        if (closeButtonPrefab != null)
        {
            Button closeButton = closeButtonPrefab.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
                Debug.Log("[Inventory] Close button successfully connected!");
            }
            else
            {
                Debug.LogWarning("[Inventory] Close button prefab doesn't have Button component!");
            }
        }
        else
        {
            Debug.LogWarning("[Inventory] Close button prefab not assigned!");
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
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventorySlotContainer);
            slotInstances.Add(slotGO);

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

            var emptySlot = new Slot
            {
                type = CollectableType.NONE,
                icon = null,
                count = 0,
                maxAllowed = 99
            };
            slots.Add(emptySlot);
        }
    }

    private IEnumerator GetInventoryData(int playerId = 0)
    {
        Debug.Log("📡 [Inventory] Đang gọi API để load data...");
        string apiUrl = $"{ApiClient.BaseUrl}/inventory/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(apiUrl.Replace("{playerId}", playerId.ToString()));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(json);

            foreach (var apiSlot in response.data)
            {
                if (apiSlot.slotIndex >= 0 && apiSlot.slotIndex < totalSlots)
                {
                    var slot = slots[apiSlot.slotIndex];
                    slot.type = System.Enum.TryParse<CollectableType>(apiSlot.collectableType, out var parsedType) ? parsedType : CollectableType.NONE;
                    slot.icon = Resources.Load<Sprite>($"Sprites/{apiSlot.icon}");
                    slot.count = apiSlot.quantity;

                    Slot_UI slotUI = slotInstances[apiSlot.slotIndex].GetComponent<Slot_UI>();
                    slotUI.Setup(apiSlot);
                }
            }
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
            StartCoroutine(SaveAllSlots());
        }
    }

    private IEnumerator SaveAllSlots()
    {
        if (!hasChanges) yield break;

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

        yield return StartCoroutine(SendBatchUpdateRequest(batchRequest));
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
        string apiUrl = $"{ApiClient.BaseUrl}/inventory/batch-update/{playerId}";
        string json = JsonUtility.ToJson(request);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "PUT");
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            hasChanges = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (hasChanges)
        {
            StartCoroutine(SaveAllSlotsSync());
        }
    }

    private IEnumerator SaveAllSlotsSync()
    {
        isAutoSaveEnabled = false;

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
        string apiUrl = $"{ApiClient.BaseUrl}/inventory/batch-update/{playerId}";
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
        }
    }



}

