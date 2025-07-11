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

    /// <summary>
    /// Lấy current stage để lưu vào server
    /// </summary>
    public int GetCurrentStage()
    {
        return currentStage;
    }

    /// <summary>
    /// Lấy timer để lưu vào server
    /// </summary>
    public float GetTimer()
    {
        return timer;
    }

    /// <summary>
    /// Restore plant state từ server data
    /// </summary>
    public void RestoreFromSaveData(PlantData plantData)
    {
        currentStage = plantData.currentStage;
        timer = plantData.timer;
        isMature = plantData.isMature;

        // Set sprite tương ứng với stage
        if (cropData != null && cropData.growthStages != null &&
            currentStage < cropData.growthStages.Length &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = cropData.growthStages[currentStage];
        }

        // Update tile state nếu mature
        if (isMature && tileManager != null)
        {
            tileManager.SetTileState(myCellPosition, TileState.Harvested);
        }
    }

    /// <summary>
    /// Calculate remaining time until plant is fully mature
    /// </summary>
    public float GetRemainingGrowthTime()
    {
        if (isMature) return 0f;
        if (cropData == null || cropData.stageDurations == null) return 0f;

        float totalRemainingTime = 0f;

        // Add remaining time for current stage
        if (currentStage < cropData.stageDurations.Length)
        {
            float remainingCurrentStage = cropData.stageDurations[currentStage] - timer;
            totalRemainingTime += remainingCurrentStage;
        }

        // Add time for all future stages
        for (int i = currentStage + 1; i < cropData.stageDurations.Length; i++)
        {
            totalRemainingTime += cropData.stageDurations[i];
        }

        return Mathf.Max(0f, totalRemainingTime);
    }

    /// <summary>
    /// Get formatted remaining time as string (e.g., "2m 30s", "45s")
    /// </summary>
    public string GetFormattedRemainingTime()
    {
        float remainingSeconds = GetRemainingGrowthTime();

        if (remainingSeconds <= 0) return "Ready!";

        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);

        if (minutes > 0)
        {
            return $"{minutes}m {seconds}s";
        }
        else
        {
            return $"{seconds}s";
        }
    }

    /// <summary>
    /// Get current growth progress as percentage (0-100)
    /// </summary>
    public float GetGrowthProgress()
    {
        if (isMature) return 100f;
        if (cropData == null || cropData.stageDurations == null) return 0f;

        // Calculate total duration for all stages
        float totalDuration = 0f;
        for (int i = 0; i < cropData.stageDurations.Length; i++)
        {
            totalDuration += cropData.stageDurations[i];
        }

        // Calculate elapsed time
        float elapsedTime = 0f;
        for (int i = 0; i < currentStage; i++)
        {
            elapsedTime += cropData.stageDurations[i];
        }
        elapsedTime += timer; // Add current stage progress

        if (totalDuration <= 0) return 0f;

        return Mathf.Clamp01(elapsedTime / totalDuration) * 100f;
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

    /// <summary>
    /// Chỉ sinh ra phần quả (không bao gồm hạt giống), và cây sẽ lùi về
    /// giai đoạn trước đó (isMature = false).
    /// </summary>
    public void HarvestFruitOnly()
    {
        if (!isMature) return;

        // Giả sử trong harvestDrops bạn đánh dấu loại nào là “quả”
        foreach (var drop in cropData.harvestDrops)
        {
            if (drop.isFruit)    // bạn cần thêm 1 flag trong HarvestDrop
            {
                for (int i = 0; i < drop.quantity; i++)
                {
                    Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
                    Instantiate(drop.itemPrefab, spawnPos, Quaternion.identity);
                }
            }
        }

        // Lùi về giai đoạn trước (ví dụ stage cuối - 1)
        currentStage = Mathf.Max(0, currentStage - 1);
        spriteRenderer.sprite = cropData.growthStages[currentStage];
        isMature = false;
        timer = 0f;  // reset đếm lại từ giai đoạn mới
    }

    /// <summary>
    /// Apply watering to directly reduce current stage time
    /// </summary>
    public float ApplyWateringReduction(float reductionPercent)
    {
        if (isMature) return 0f; // Can't water mature plants
        if (currentStage >= cropData.growthStages.Length - 1) return 0f; // Already at final stage

        // Calculate remaining time for current stage
        float remainingTime = cropData.stageDurations[currentStage] - timer;

        // Calculate time to reduce (30% of remaining time)
        float timeToReduce = remainingTime * reductionPercent;

        // Apply the reduction by advancing the timer
        timer += timeToReduce;

        // Ensure timer doesn't exceed stage duration
        timer = Mathf.Min(timer, cropData.stageDurations[currentStage]);

        return timeToReduce;
    }
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
// }