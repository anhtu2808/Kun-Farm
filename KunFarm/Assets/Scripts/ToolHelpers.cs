using UnityEngine;

/// <summary>
/// Helper methods để convert giữa Tools và CollectableTypes
/// </summary>
public static class ToolHelpers
{
    /// <summary>
    /// Convert CollectableType thành Tool object
    /// </summary>
    public static Tool CreateToolFromCollectable(CollectableType collectableType, Sprite icon = null, int quantity = 1)
    {
        Tool tool = null;
        
        switch (collectableType)
        {
            case CollectableType.SHOVEL_TOOL:
                tool = new ShovelTool(quantity); // Durability = quantity
                break;
                
            case CollectableType.HAND_TOOL:
                tool = new HandTool(); // Infinite use
                break;
                
            case CollectableType.WHEATSEED:
                tool = CreateSeedTool("Wheat", icon, quantity);
                break;
                
            case CollectableType.GRAPESEED:
                tool = CreateSeedTool("Grapes", icon, quantity);
                break;
                
            case CollectableType.APPLETREESEED:
                tool = CreateSeedTool("AppleTree", icon, quantity);
                break;
        }
        
        if (tool != null && icon != null)
        {
            tool.toolIcon = icon;
        }
        
        return tool;
    }
    
    /// <summary>
    /// Convert Tool object thành CollectableType
    /// </summary>
    public static CollectableType GetCollectableFromTool(Tool tool)
    {
        switch (tool)
        {
            case ShovelTool _:
                return CollectableType.SHOVEL_TOOL;
                
            case HandTool _:
                return CollectableType.HAND_TOOL;
                
            case SeedTool seedTool when seedTool.cropData != null:
                return GetSeedCollectableFromCropData(seedTool.cropData);
                
            default:
                return CollectableType.NONE;
        }
    }
    
    /// <summary>
    /// Check if CollectableType có thể thành tool
    /// </summary>
    public static bool CanBeTool(CollectableType collectableType)
    {
        return collectableType == CollectableType.SHOVEL_TOOL ||
               collectableType == CollectableType.HAND_TOOL ||
               collectableType == CollectableType.WHEATSEED ||
               collectableType == CollectableType.GRAPESEED ||
               collectableType == CollectableType.APPLETREESEED;
    }
    
    /// <summary>
    /// Check if CollectableType có thể trồng cây trực tiếp
    /// </summary>
    public static bool CanPlantDirectly(CollectableType collectableType)
    {
        return collectableType == CollectableType.WHEATSEED ||
               collectableType == CollectableType.WHEAT ||
               collectableType == CollectableType.GRAPESEED ||
               collectableType == CollectableType.GRAPE ||
               collectableType == CollectableType.APPLETREESEED ||
               collectableType == CollectableType.APPLETREE;
    }
    
    /// <summary>
    /// Plant crop trực tiếp từ collectable (không cần tool)
    /// </summary>
    public static bool PlantFromCollectable(CollectableType collectableType, Vector3Int cellPosition, TileManager tileManager)
    {
        if (!CanPlantDirectly(collectableType)) return false;
        if (tileManager.GetTileState(cellPosition) != TileState.Dug) return false;
        
        // Get CropData từ collectable
        CropData cropData = GetCropDataFromCollectable(collectableType);
        if (cropData == null || cropData.cropPrefab == null) return false;
        
        // Trồng cây
        Vector3 worldPos = tileManager.GetTilemap().GetCellCenterWorld(cellPosition);
        GameObject newPlant = Object.Instantiate(cropData.cropPrefab, worldPos, Quaternion.identity);
        newPlant.name = cropData.cropName + " Plant";

        CropGrower cropGrower = newPlant.GetComponent<CropGrower>();
        if (cropGrower != null)
        {
            cropGrower.SetTilePosition(cellPosition);
            tileManager.RegisterPlant(cellPosition, newPlant);
            tileManager.SetTileState(cellPosition, TileState.Planted);
            return true;
        }
        else
        {
            Object.Destroy(newPlant);
            return false;
        }
    }
    
    private static Tool CreateSeedTool(string cropDataName, Sprite icon, int quantity)
    {
        CropData cropData = Resources.Load<CropData>($"CropData/{cropDataName}");
        if (cropData != null)
        {
            var seedTool = new SeedTool(cropData, quantity);
            seedTool.toolIcon = icon;
            return seedTool;
        }
        return null;
    }
    
    private static CropData GetCropDataFromCollectable(CollectableType collectableType)
    {
        switch (collectableType)
        {
            case CollectableType.WHEATSEED:
            case CollectableType.WHEAT:
                return Resources.Load<CropData>("CropData/Wheat");
            case CollectableType.GRAPESEED:
            case CollectableType.GRAPE:
                return Resources.Load<CropData>("CropData/Grapes");
            case CollectableType.APPLETREESEED:
            case CollectableType.APPLETREE:
                return Resources.Load<CropData>("CropData/AppleTree");
            default:
                return null;
        }
    }
    
    private static CollectableType GetSeedCollectableFromCropData(CropData cropData)
    {
        // Dùng asset name thay vì cropName vì cropName có thể trống
        string assetName = cropData.name.ToLower();
        
        if (assetName.Contains("wheat")) return CollectableType.WHEATSEED;
        if (assetName.Contains("grape")) return CollectableType.GRAPESEED;
        if (assetName.Contains("apple")) return CollectableType.APPLETREESEED;
        
        // Fallback: thử cropName nếu asset name không match
        if (!string.IsNullOrEmpty(cropData.cropName))
        {
            string cropName = cropData.cropName.ToLower();
            if (cropName.Contains("wheat")) return CollectableType.WHEATSEED;
            if (cropName.Contains("grape")) return CollectableType.GRAPESEED;
            if (cropName.Contains("apple")) return CollectableType.APPLETREESEED;
        }
        
        return CollectableType.NONE;
    }
} 