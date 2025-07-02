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

        public void AddItem(Collectable item)
        {
            this.type = item.type;
            this.icon = item.icon;
            count++;
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
        StartCoroutine(GetInventoryData(1));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
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

    private IEnumerator GetInventoryData(int playerId = 1)
    {
        string apiUrl = "https://localhost:7067/inventory/{playerId}";
        UnityWebRequest request = UnityWebRequest.Get(apiUrl.Replace("{playerId}", playerId.ToString()));
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            InventoryResponse response = JsonUtility.FromJson<InventoryResponse>(json);

            // Nếu chưa có đủ slot -> tạo
            while (slotInstances.Count < totalSlots)
            {
                GameObject slotGO = Instantiate(inventorySlotPrefab, inventorySlotContainer);
                slotInstances.Add(slotGO);
            }

            // Lặp 27 lần để setup từng slot
            for (int i = 0; i < totalSlots; i++)
            {
                InventorySlotData data;
                if (i < response.data.Count)
                    data = response.data[i];
                else
                {
                    data = new InventorySlotData
                    {
                        id = -1,
                        slotIndex = i,
                        itemId = 0,
                        collectableType = "NONE",
                        icon = "NONE",
                        quantity = 0
                    };
                }

                Slot_UI slotUI = slotInstances[i].GetComponent<Slot_UI>();
                slotUI.Setup(data);
                StartCoroutine(DelayedInitDragDrop(slotUI, i));
                var slot = new Slot
                {
                    type = System.Enum.TryParse<CollectableType>(data.collectableType, out var parsedType) ? parsedType : CollectableType.NONE,
                    icon = Resources.Load<Sprite>($"Sprites/{data.icon}"),
                    count = data.quantity,
                    maxAllowed = 99
                };
                slots.Add(slot);
            }
        }
        else
        {
            Debug.LogError("Lỗi khi gọi inventory API: " + request.error);
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
    public void Add(Collectable item)
    {
        foreach (Slot slot in slots)
        {
            if (slot.type == item.type && slot.CanAddItem())
            {
                slot.AddItem(item);
                onInventoryChanged?.Invoke();
                return;
            }
        }

        foreach (Slot slot in slots)
        {
            if (slot.type == CollectableType.NONE)
            {
                slot.AddItem(item);
                onInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void Remove(int index)
    {
        slots[index].RemoveItem();
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
        foreach (Slot slot in slots)
        {
            if (slot.type == type && slot.CanAddItem())
            {
                slot.type = type;
                slot.icon = icon;
                slot.count += count;
                onInventoryChanged?.Invoke();
                return;
            }
        }

        // Find empty slot
        foreach (Slot slot in slots)
        {
            if (slot.type == CollectableType.NONE)
            {
                slot.type = type;
                slot.icon = icon;
                slot.count = count;
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
            onInventoryChanged?.Invoke();
        }
    }

    // Method to trigger inventory changed event from external scripts
    public void NotifyInventoryChanged()
    {
        onInventoryChanged?.Invoke();
    }
    private bool saveCompleted = false;

    private void OnApplicationQuit()
    {
        Debug.Log("▶️ [Quit] App đang thoát... Lưu dữ liệu!");

        List<SaveInventoryRequest> saveRequests = new List<SaveInventoryRequest>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].type != CollectableType.NONE)
            {
                SaveInventoryRequest request = new SaveInventoryRequest
                {
                    SlotIndex = i,
                    CollectableType = slots[i].type.ToString(),
                    quantity = slots[i].count
                };
                saveRequests.Add(request);
            }
            else
            {
                SaveInventoryRequest request = new SaveInventoryRequest
                {
                    SlotIndex = i,
                    CollectableType = CollectableType.NONE.ToString(),
                    quantity = 0
                };
                saveRequests.Add(request);
            }
        }

        Debug.Log($"📦 [Quit] Tổng số request chuẩn bị gửi: {saveRequests.Count}");

        SaveGameData(saveRequests);

        float timeout = 5f;
        float startTime = Time.realtimeSinceStartup;
        Debug.Log("⏳ [Quit] Đang chờ gửi inventory...");
        while (!saveCompleted && Time.realtimeSinceStartup - startTime < timeout)
        {
            System.Threading.Thread.Sleep(100);
        }

        if (saveCompleted)
            Debug.Log("✔️ [Quit] Gửi thành công trước khi thoát ứng dụng.");
        else
            Debug.LogWarning("⚠️ [Quit] Hết thời gian chờ, có thể dữ liệu chưa được gửi xong.");
    }

    private void SaveGameData(List<SaveInventoryRequest> saveRequests)
    {
        try
        {
            string apiUrl = "https://localhost:7067/inventory/save/1";
            InventorySaveList requestBody = new InventorySaveList(saveRequests);
            string json = JsonUtility.ToJson(requestBody);
            Debug.Log("📤 [Blocking Send] JSON gửi: " + json);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi đồng bộ, chặn cho tới khi xong
                HttpResponseMessage response = client.PostAsync(apiUrl, content).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Debug.Log("✅ [Blocking Send] Lưu thành công! Response: " + result);
                }
                else
                {
                    Debug.LogError("❌ [Blocking Send] Lỗi khi gửi: " + response.StatusCode);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ [Blocking Send] Exception: " + ex.Message);
        }
    }

}
