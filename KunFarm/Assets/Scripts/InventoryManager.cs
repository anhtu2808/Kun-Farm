// InventoryManager.cs
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; } // Singleton Instance

    public Inventory playerInventory; // Inventory thực tế của người chơi (sẽ hiển thị trong Inspector)

    [SerializeField] private int initialSlotCount = 10; // Số lượng slot ban đầu

    void Awake()
    {
        // Logic Singleton
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Tùy chọn: nếu bạn muốn InventoryManager tồn tại giữa các Scene
        }
        else
        {
            Destroy(gameObject); // Chỉ cho phép một instance
        }

        // Khởi tạo Inventory nếu chưa có (khi game bắt đầu)
        if (playerInventory == null)
        {
            playerInventory = new Inventory(initialSlotCount);
        }
    }

    // Hàm tiện ích để thêm item vào Inventory của người chơi
    public void AddItemToPlayerInventory(Collectable item)
    {
        if (playerInventory != null)
        {
            playerInventory.Add(item);
        }
        else
        {
            Debug.LogError("Player Inventory is null in InventoryManager!");
        }
    }

    // Có thể thêm các hàm tiện ích khác như GetItemCount, HasItem, RemoveItem, v.v.
}