using UnityEngine;

/// <summary>
/// ScriptableObject để define tools và properties của chúng
/// </summary>
[CreateAssetMenu(fileName = "NewToolData", menuName = "Tools/Tool Data")]
public class ToolData : ScriptableObject
{
    [Header("Tool Info")]
    public string toolName;
    public Sprite toolIcon;
    public ToolType toolType;
    public int animatorToolIndex;

    [Header("Seed Tool Only")]
    public CropData cropData; // Chỉ dùng cho SeedTool

    public Tool CreateTool()
    {
        Tool tool = null;
        
        switch (toolType)
        {
            case ToolType.Shovel:
                tool = new ShovelTool();
                break;
            case ToolType.Hand:
                tool = new HandTool();
                break;
            case ToolType.Seed:
                tool = new SeedTool(cropData);
                break;
        }

        if (tool != null)
        {
            tool.toolName = this.toolName;
            tool.toolIcon = this.toolIcon;
            tool.animatorToolIndex = this.animatorToolIndex;
        }

        return tool;
    }
}

public enum ToolType
{
    Shovel,
    Hand,
    Seed
} 