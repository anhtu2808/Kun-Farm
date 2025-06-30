// CropGrower.cs
using UnityEngine;

public class CropGrower : MonoBehaviour
{
    public CropData cropData; // Kéo CropData Asset của cây này vào đây (trên Prefab Grape)
    private SpriteRenderer spriteRenderer;
    private int currentStage = 0;
    private float timer = 0f;
    public bool isMature = false; // Đổi thành public để PlayerInteraction có thể đọc
    // private bool playerNearby = false; // Để kiểm tra player có gần không (nếu bạn dùng logic này)

    // Thêm tham chiếu đến TileManager và biến lưu vị trí ô
    private TileManager tileManager;
    private Vector3Int myCellPosition; // Vị trí ô của cây này trên Tilemap

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("CropGrower: SpriteRenderer component not found on this GameObject.", this);
            return;
        }

        // Lấy tham chiếu đến TileManager ngay trong Awake
        tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("TileManager not found in the scene! Crop growth state won't update tile status.", this);
        }

        // Kiểm tra và khởi tạo CropData
        if (cropData == null)
        {
            Debug.LogError("CropData is null for CropGrower on " + gameObject.name + "! Make sure to assign it on the Prefab.", this);
            return;
        }

        // Gán sprite đầu tiên từ CropData
        if (cropData.growthStages != null && cropData.growthStages.Length > 0 && cropData.growthStages[0] != null)
        {
            spriteRenderer.sprite = cropData.growthStages[0];
        }
        else
        {
            Debug.LogError("CropData for " + cropData.cropName + " has no initial sprite or growth stages!", this);
        }

        // Reset trạng thái ban đầu của cây
        currentStage = 0;
        timer = 0f;
        isMature = false;
    }

    // Hàm này được gọi từ PlayerInteraction để thiết lập vị trí ô của cây
    public void SetTilePosition(Vector3Int pos)
    {
        myCellPosition = pos;
    }

    void Update()
    {
        // Cây phát triển qua từng stage
        // Chỉ chạy nếu cropData không null (đã được gán) và cây chưa trưởng thành
        if (cropData != null && !isMature && currentStage < cropData.growthStages.Length - 1)
        {
            timer += Time.deltaTime;
            if (timer >= cropData.stageDurations[currentStage])
            {
                currentStage++;
                // Kiểm tra để tránh lỗi IndexOutOfRangeException nếu mảng bị cấu hình sai
                if (currentStage < cropData.growthStages.Length)
                {
                    spriteRenderer.sprite = cropData.growthStages[currentStage];
                }
                else
                {
                    Debug.LogWarning("GrowthStages array is too short for currentStage " + currentStage + " for crop " + cropData.cropName, this);
                }
                timer = 0f;

                // Nếu đạt đến giai đoạn cuối cùng, đánh dấu là trưởng thành và cập nhật trạng thái ô đất
                if (currentStage == cropData.growthStages.Length - 1)
                {
                    isMature = true;
                    // === QUAN TRỌNG: CẬP NHẬT TRẠNG THÁI CỦA Ô ĐẤT Ở ĐÂY ===
                    if (tileManager != null)
                    {
                        tileManager.SetTileState(myCellPosition, TileState.Harvested);
                    }
                }
            }
        }
    }

    // Hàm Harvest() được gọi từ PlayerInteraction
    public void Harvest() // Đảm bảo là public
    {

        // Sinh ra các item rơi ra
        foreach (HarvestDrop drop in cropData.harvestDrops)
        {
           for (int i = 0; i < drop.quantity; i++)
        {
            // Vị trí rơi ngẫu nhiên quanh cây
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;

            if (drop.itemPrefab != null)
            {
                Instantiate(drop.itemPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("itemPrefab trong HarvestDrop chưa được gán!");
            }
        }
        }
        // GameObject cây sẽ được phá hủy bởi TileManager/PlayerInteraction sau khi Deregister
    }

    // Logic kiểm tra người chơi lại gần (nếu bạn muốn thu hoạch khi người chơi ở gần)
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerNearby = true;
    //     }
    // }

    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerNearby = false;
    //     }
    // }
}