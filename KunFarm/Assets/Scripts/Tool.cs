using UnityEngine;

/// <summary>
/// Base class cho tất cả tools trong game
/// </summary>
[System.Serializable]
public abstract class Tool
{
    public string toolName;
    public Sprite toolIcon;
    public int animatorToolIndex; // Index cho animator
    public int quantity = 1; // Số lượng/độ bền của tool

    public abstract void Use(Vector3Int cellPosition, TileManager tileManager);
    public abstract bool CanUse(Vector3Int cellPosition, TileManager tileManager);
    
    /// <summary>
    /// Consume tool khi sử dụng. Return true nếu tool còn sử dụng được
    /// </summary>
    public virtual bool ConsumeOnUse()
    {
        return true; // Mặc định không consume
    }
    
    /// <summary>
    /// Check xem tool có thể bị consume không
    /// </summary>
    public virtual bool IsConsumable()
    {
        return false; // Mặc định không consumable
    }
}

/// <summary>
/// Tool để đào đất
/// </summary>
[System.Serializable]
public class ShovelTool : Tool
{
    public ShovelTool(int durability = 10)
    {
        toolName = "Shovel";
        animatorToolIndex = 1; // Index for shovel tool in animator
        quantity = durability; // Độ bền của xẻng
    }

    public override void Use(Vector3Int cellPosition, TileManager tileManager)
    {
        TileState currentState = tileManager.GetTileState(cellPosition);
        if (currentState == TileState.Undug)
        {
            tileManager.SetTileState(cellPosition, TileState.Dug);
        }
    }

    public override bool CanUse(Vector3Int cellPosition, TileManager tileManager)
    {
        return quantity > 0 && tileManager.GetTileState(cellPosition) == TileState.Undug;
    }
    
    public override bool ConsumeOnUse()
    {
        quantity--;
        return quantity > 0; // Return false nếu hết độ bền
    }
    
    public override bool IsConsumable()
    {
        return true; // Shovel có thể hết độ bền
    }
}

/// <summary>
/// Tool để trồng cây với crop data cụ thể
/// </summary>
[System.Serializable]
public class SeedTool : Tool
{
    public CropData cropData;

    public SeedTool(CropData crop, int seedCount = 1)
    {
        cropData = crop;
        toolName = crop ? crop.cropName + " Seed" : "Seed";
        animatorToolIndex = 2;
        quantity = seedCount; // Số lượng hạt giống
    }

    public override void Use(Vector3Int cellPosition, TileManager tileManager)
    {
        if (cropData == null || cropData.cropPrefab == null) return;

        // Trồng cây
        Vector3 worldPos = tileManager.GetTilemap().GetCellCenterWorld(cellPosition);
        GameObject newPlant = Object.Instantiate(cropData.cropPrefab, worldPos, Quaternion.identity);
        newPlant.name = cropData.cropName + " Plant";

        CropGrower cropGrower = newPlant.GetComponent<CropGrower>();
        if (cropGrower != null)
        {
            // IMPORTANT: Explicitly set cropData on the instantiated plant
            cropGrower.cropData = cropData;
            cropGrower.SetTilePosition(cellPosition);
            tileManager.RegisterPlant(cellPosition, newPlant);
            tileManager.SetTileState(cellPosition, TileState.Planted);
            Debug.Log($"[SeedTool] ✅ Planted {cropData.name} at ({cellPosition.x}, {cellPosition.y}) with cropData set");
        }
        else
        {
            Object.Destroy(newPlant);
        }
    }

    public override bool CanUse(Vector3Int cellPosition, TileManager tileManager)
    {
        return quantity > 0 &&
               cropData != null && 
               cropData.cropPrefab != null && 
               tileManager.GetTileState(cellPosition) == TileState.Dug;
    }
    
    public override bool ConsumeOnUse()
    {
        quantity--;
        return quantity > 0; // Return false nếu hết hạt giống
    }
    
    public override bool IsConsumable()
    {
        return true; // Seed có thể hết
    }
}

/// <summary>
/// Tool để thu hoạch cây
/// </summary>
[System.Serializable]
public class HandTool : Tool
{
    public HandTool()
    {
        toolName = "Hand";
        animatorToolIndex = 3;
        quantity = -1; // -1 = vô hạn
    }

    public override void Use(Vector3Int cellPosition, TileManager tileManager)
    {
        GameObject plant = tileManager.GetPlantAt(cellPosition);
        if (plant != null)
        {
            CropGrower cropGrower = plant.GetComponent<CropGrower>();
            if (cropGrower != null && cropGrower.isMature)
            {
                cropGrower.Harvest();
                tileManager.DeregisterPlant(cellPosition);
                tileManager.SetTileState(cellPosition, TileState.Dug);
                Object.Destroy(plant);
            }
        }
    }

    public override bool CanUse(Vector3Int cellPosition, TileManager tileManager)
    {
        GameObject plant = tileManager.GetPlantAt(cellPosition);
        if (plant != null)
        {
            CropGrower cropGrower = plant.GetComponent<CropGrower>();
            return cropGrower != null && cropGrower.isMature;
        }
        return false;
    }
    
    public override bool ConsumeOnUse()
    {
        return true; // Hand tool không bao giờ hết
    }
    
    public override bool IsConsumable()
    {
        return false; // Hand tool vĩnh viễn
    }
} 

/// <summary>
/// Tool để ăn food và restore health/hunger
/// </summary>
[System.Serializable]
public class FoodTool : Tool
{
    [Header("Food Properties")]
    public float hungerRestore = 50f;
    public float healthRestore = 30f;
    
    public FoodTool(int foodCount = 1)
    {
        toolName = "Food";
        animatorToolIndex = 4; // Index cho eating animation
        quantity = foodCount;
    }

    public override void Use(Vector3Int cellPosition, TileManager tileManager)
    {
        // Food tool không cần cellPosition hoặc tileManager
        // Logic eating sẽ được handle trong ToolManager
    }

    public override bool CanUse(Vector3Int cellPosition, TileManager tileManager)
    {
        // Food luôn có thể dùng miễn là còn quantity
        return quantity > 0;
    }
    
    public override bool ConsumeOnUse()
    {
        quantity--;
        return quantity > 0; // Return false nếu hết food
    }
    
    public override bool IsConsumable()
    {
        return true; // Food có thể hết
    }
    
    /// <summary>
    /// Eat food và restore player stats
    /// </summary>
    public void EatFood()
    {
        PlayerStats playerStats = Object.FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.RestoreHunger(hungerRestore);
            playerStats.RestoreHealth(healthRestore);
            
            SimpleNotificationPopup.Show($"Ate {toolName}! +{hungerRestore} Hunger, +{healthRestore} Health");
            Debug.Log($"[FoodTool] Ate {toolName}: +{hungerRestore} hunger, +{healthRestore} health");
        }
        else
        {
            Debug.LogWarning("[FoodTool] PlayerStats not found!");
        }
    }
} 