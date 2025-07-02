using System.ComponentModel.DataAnnotations;

namespace KunFarm.BLL.DTOs.Request
{
    public class UpdateInventorySlotRequest
    {
        [Required]
        public int SlotIndex { get; set; }
        
        [Required]
        public string CollectableType { get; set; } = string.Empty;
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
} 