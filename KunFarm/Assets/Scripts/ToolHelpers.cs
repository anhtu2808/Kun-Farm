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
                // Try to load default icon if no icon provided
                if (icon == null)
                {
                    icon = Resources.Load<Sprite>("Sprites/hand_icon") ?? 
                           Resources.Load<Sprite>("Tools/hand_icon") ??
                           Resources.Load<Sprite>("hand_icon") ??
                           Resources.Load<Sprite>("Sprites/hand") ??
                           Resources.Load<Sprite>("hand");
                }
                break;
                
            case CollectableType.WHEATSEED:
            case CollectableType.GRAPESEED:
            case CollectableType.APPLETREESEED:
                tool = CreateSeedToolFromType(collectableType, quantity);
                break;
                
            case CollectableType.APPLE:
            case CollectableType.WHEAT:
            case CollectableType.GRAPE:
                tool = new FoodTool(quantity);
                break;
                
            default:
                break;
        }
        
        if (tool != null)
        {
            tool.toolIcon = icon;
            string toolTypeName = GetToolTypeName(collectableType);
            tool.toolName = $"{toolTypeName}";
        }
        
        return tool;
    }
    
    /// <summary>
    /// Convert Tool thành CollectableType (reverse operation)
    /// </summary>
    public static CollectableType GetCollectableFromTool(Tool tool)
    {
        if (tool == null) return CollectableType.NONE;
        
        if (tool is ShovelTool) return CollectableType.SHOVEL_TOOL;
        if (tool is HandTool) return CollectableType.HAND_TOOL;
                
        // For SeedTool, determine type from cropData
        if (tool is SeedTool seedTool && seedTool.cropData != null)
        {
            string cropName = seedTool.cropData.name.ToLower();
            if (cropName.Contains("wheat")) return CollectableType.WHEATSEED;
            if (cropName.Contains("grape")) return CollectableType.GRAPESEED;
            if (cropName.Contains("apple")) return CollectableType.APPLETREESEED;
            return CollectableType.WHEATSEED; // Default fallback
        }
        
        // For FoodTool, we need to determine type from name
        if (tool is FoodTool)
        {
            if (tool.toolName != null)
            {
                if (tool.toolName.Contains("Apple")) return CollectableType.APPLE;
                if (tool.toolName.Contains("Wheat")) return CollectableType.WHEAT;
                if (tool.toolName.Contains("Grape")) return CollectableType.GRAPE;
            }
            // Default fallback for FoodTool
            return CollectableType.APPLE;
        }
        
                return CollectableType.NONE;
    }
    
    /// <summary>
    /// Kiểm tra xem CollectableType có thể trở thành Tool không
    /// </summary>
    public static bool CanBeTool(CollectableType collectableType)
    {
        bool canBeTool = false;
        
        switch (collectableType)
        {
            // Tool items
            case CollectableType.SHOVEL_TOOL:
            case CollectableType.HAND_TOOL:
                canBeTool = true;
                break;
                
            // Seed items có thể trở thành seed tools
            case CollectableType.WHEATSEED:
            case CollectableType.GRAPESEED:
            case CollectableType.APPLETREESEED:
                canBeTool = true;
                break;
                
            // Food items có thể trở thành food tools
            case CollectableType.APPLE:
            case CollectableType.WHEAT:
            case CollectableType.GRAPE:
                canBeTool = true;
                break;
                
            default:
                canBeTool = false;
                break;
        }
        
        return canBeTool;
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
    /// Check if CollectableType là food
    /// </summary>
    public static bool IsFood(CollectableType collectableType)
    {
        return collectableType == CollectableType.APPLE ||
               collectableType == CollectableType.WHEAT ||
               collectableType == CollectableType.GRAPE;
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
            // IMPORTANT: Explicitly set cropData on the instantiated plant
            cropGrower.cropData = cropData;
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
    
    /// <summary>
    /// Lấy tên tool từ CollectableType
    /// </summary>
    private static string GetToolTypeName(CollectableType collectableType)
    {
        switch (collectableType)
    {
            case CollectableType.SHOVEL_TOOL: return "Shovel";
            case CollectableType.HAND_TOOL: return "Hand";
            case CollectableType.WHEATSEED: return "Wheat Seed";
            case CollectableType.GRAPESEED: return "Grape Seed";
            case CollectableType.APPLETREESEED: return "Apple Tree Seed";
            case CollectableType.APPLE: return "Apple";
            case CollectableType.WHEAT: return "Wheat";
            case CollectableType.GRAPE: return "Grape";
            default: return collectableType.ToString();
        }
    }
    
    /// <summary>
    /// Tạo SeedTool từ CollectableType
    /// </summary>
    private static Tool CreateSeedToolFromType(CollectableType collectableType, int quantity)
    {
        CropData cropData = null;
        
        switch (collectableType)
        {
            case CollectableType.WHEATSEED:
                cropData = Resources.Load<CropData>("CropData/Wheat");
                break;
                
            case CollectableType.GRAPESEED:
                cropData = Resources.Load<CropData>("CropData/Grapes");
                break;
                
            case CollectableType.APPLETREESEED:
                cropData = Resources.Load<CropData>("CropData/AppleTree");
                break;
        }
        
        if (cropData != null)
        {
            return new SeedTool(cropData, quantity);
        }
        
        return null;
    }
} 