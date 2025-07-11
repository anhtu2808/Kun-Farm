using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToolbarSaveData
{
    public int userId;
    public List<ToolSlotSaveData> tools = new List<ToolSlotSaveData>();

    public static ToolbarSaveData FromToolManager(ToolManager toolManager, int userId)
    {
        var data = new ToolbarSaveData();
        data.userId = userId;

        for (int i = 0; i < 9; i++) // 9 toolbar slots
        {
            Tool tool = toolManager.GetToolAtIndex(i);
            if (tool != null)
            {
                var toolData = new ToolSlotSaveData
                {
                    slotIndex = i,
                    toolType = tool.GetType().Name, // "ShovelTool", "WateringCanTool", etc.
                    toolName = tool.toolName,
                    quantity = tool.quantity,
                    animatorToolIndex = tool.animatorToolIndex,
                    iconPath = tool.toolIcon != null ? tool.toolIcon.name : ""
                };
                data.tools.Add(toolData);
            }
        }

        return data;
    }

    public void ApplyToToolManager(ToolManager toolManager)
    {
        if (toolManager == null) return;

        // Clear all toolbar slots first
        for (int i = 0; i < 9; i++)
        {
            toolManager.SetToolAtIndex(i, null);
        }

        // Luôn đặt Hand Tool ở slot đầu tiên (index 0) - để ToolManager tự tạo với icon
        toolManager.EnsureHandTool();

        // Apply saved tools, skip nếu saved tool ở slot 0 là HandTool
        foreach (var toolData in tools)
        {
            if (toolData.slotIndex >= 0 && toolData.slotIndex < 9)
            {
                // Skip slot 0 nếu là HandTool (đã được set)
                if (toolData.slotIndex == 0 && toolData.toolType == "HandTool")
                {
                    continue;
                }
                
                Tool tool = CreateToolFromData(toolData);
                if (tool != null)
                {
                    // Nếu saved tool là HandTool nhưng không ở slot 0, move it to next available slot
                    if (tool is HandTool && toolData.slotIndex != 0)
                    {
                        // Find next available slot
                        for (int i = 1; i < 9; i++)
                        {
                            if (toolManager.GetToolAtIndex(i) == null)
                            {
                                toolManager.SetToolAtIndex(i, tool);
                                break;
                            }
                        }
                    }
                    else
                    {
                        toolManager.SetToolAtIndex(toolData.slotIndex, tool);
                    }
                }
            }
        }

        // Update toolbar display
        toolManager.UpdateToolbarDisplay();
        
        Debug.Log("[ToolbarSaveData] Hand Tool đã được đặt ở slot đầu tiên khi load");
    }

    private Tool CreateToolFromData(ToolSlotSaveData data)
    {
        try
        {
            switch (data.toolType)
            {
                case "ShovelTool":
                    var shovel = new ShovelTool(data.quantity);
                    shovel.toolName = data.toolName;
                    shovel.animatorToolIndex = data.animatorToolIndex;
                    LoadToolIcon(shovel, data.iconPath);
                    return shovel;

                // WateringCanTool not available in this project

                case "HandTool":
                    var hand = new HandTool();
                    hand.toolName = data.toolName;
                    hand.animatorToolIndex = data.animatorToolIndex;
                    LoadToolIcon(hand, data.iconPath);
                    return hand;

                case "FoodTool":
                    var food = new FoodTool(data.quantity);
                    food.toolName = data.toolName;
                    food.animatorToolIndex = data.animatorToolIndex;
                    LoadToolIcon(food, data.iconPath);
                    return food;

                case "WateringCanTool":
                    var wateringCan = new WateringCanTool(data.quantity);
                    wateringCan.toolName = data.toolName;
                    wateringCan.animatorToolIndex = data.animatorToolIndex;
                    LoadToolIcon(wateringCan, data.iconPath);
                    return wateringCan;

                case "SeedTool":
                    // Find the crop data for seed tool
                    CropData cropData = GetCropDataFromName(data.toolName);
                    if (cropData != null)
                    {
                        var seed = new SeedTool(cropData, data.quantity);
                        seed.toolName = data.toolName;
                        seed.animatorToolIndex = data.animatorToolIndex;
                        LoadToolIcon(seed, data.iconPath);
                        return seed;
                    }
                    break;

                default:
                    Debug.LogWarning($"[ToolbarSaveData] Unknown tool type: {data.toolType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ToolbarSaveData] Error creating tool from data: {ex.Message}");
        }

        return null;
    }

    private void LoadToolIcon(Tool tool, string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath)) 
        {
            // Nếu không có iconPath, thử load default icon cho HandTool
            if (tool is HandTool)
            {
                TryLoadHandToolIcon(tool);
            }
            return;
        }

        try
        {
            // Try to load icon from Resources với nhiều path
            Sprite icon = Resources.Load<Sprite>($"Sprites/{iconPath}") ?? 
                         Resources.Load<Sprite>($"Tools/{iconPath}") ??
                         Resources.Load<Sprite>(iconPath);
            
            if (icon != null)
            {
                tool.toolIcon = icon;
            }
            else
            {
                Debug.LogWarning($"[ToolbarSaveData] Could not load icon: {iconPath}");
                
                // Fallback cho HandTool
                if (tool is HandTool)
                {
                    TryLoadHandToolIcon(tool);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ToolbarSaveData] Error loading icon {iconPath}: {ex.Message}");
            
            // Fallback cho HandTool
            if (tool is HandTool)
            {
                TryLoadHandToolIcon(tool);
            }
        }
    }

    private void TryLoadHandToolIcon(Tool handTool)
    {
        // Thử load icon từ các path có thể cho HandTool
        Sprite handIcon = Resources.Load<Sprite>("Sprites/hand_icon") ?? 
                         Resources.Load<Sprite>("Tools/hand_icon") ??
                         Resources.Load<Sprite>("hand_icon") ??
                         Resources.Load<Sprite>("Sprites/hand") ??
                         Resources.Load<Sprite>("hand");
        
        if (handIcon != null)
        {
            handTool.toolIcon = handIcon;
            Debug.Log($"[ToolbarSaveData] Loaded default HandTool icon: {handIcon.name}");
        }
        else
        {
            Debug.LogWarning("[ToolbarSaveData] No default icon found for HandTool!");
        }
    }

    private CropData GetCropDataFromName(string name)
    {
        // Map tool names to crop data
        switch (name?.ToLower())
        {
            case "apple tree seed":
            case "appletreeseed":
            case "apple seed":
                return Resources.Load<CropData>("CropData/AppleTree");
            case "grape seed":
            case "grapeseed":
                return Resources.Load<CropData>("CropData/Grapes");
            case "wheat seed":
            case "wheatseed":
                return Resources.Load<CropData>("CropData/Wheat");
            default:
                return null;
        }
    }

    private CollectableType GetCollectableTypeFromName(string name)
    {
        // Map tool names to collectable types (for compatibility)
        switch (name?.ToLower())
        {
            case "apple tree seed":
            case "appletreeseed":
            case "apple seed":
                return CollectableType.APPLETREESEED;
            case "grape seed":
            case "grapeseed":
                return CollectableType.GRAPESEED;
            case "wheat seed":
            case "wheatseed":
                return CollectableType.WHEATSEED;
            default:
                return CollectableType.NONE;
        }
    }
}

[Serializable]
public class ToolSlotSaveData
{
    public int slotIndex;
    public string toolType = "";
    public string toolName = "";
    public int quantity = 0;
    public int animatorToolIndex = 0;
    public string iconPath = "";
} 