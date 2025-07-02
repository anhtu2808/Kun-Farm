using System.ComponentModel.DataAnnotations;

namespace KunFarm.BLL.DTOs.Request
{
    public class SaveToolbarRequest
    {
        [Required]
        public int userId { get; set; }
        
        [Required]
        public List<ToolSlotData> tools { get; set; } = new List<ToolSlotData>();
    }
    
    public class ToolSlotData
    {
        public int slotIndex { get; set; }
        public string toolType { get; set; } = string.Empty; // "ShovelTool", "WateringCanTool", "FoodTool", etc.
        public string toolName { get; set; } = string.Empty;
        public int quantity { get; set; } = 0;
        public int animatorToolIndex { get; set; } = 0;
        public string iconPath { get; set; } = string.Empty; // Optional: path to icon for server reference
    }
} 