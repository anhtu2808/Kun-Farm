namespace KunFarm.BLL.DTOs.Response
{
    public class ToolbarResponse
    {
        public int PlayerStateId { get; set; }
        public List<ToolSlotResponse> Tools { get; set; } = new List<ToolSlotResponse>();
        public DateTime LastSaved { get; set; }
    }
    
    public class ToolSlotResponse
    {
        public int SlotIndex { get; set; }
        public string ToolType { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public int AnimatorToolIndex { get; set; } = 0;
        public string IconPath { get; set; } = string.Empty;
    }
} 