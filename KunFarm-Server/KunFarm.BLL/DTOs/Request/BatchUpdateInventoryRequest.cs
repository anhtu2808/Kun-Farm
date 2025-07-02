using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace KunFarm.BLL.DTOs.Request
{
    public class BatchUpdateInventoryRequest
    {
        [Required]
        public List<UpdateInventorySlotRequest> Slots { get; set; } = new List<UpdateInventorySlotRequest>();
    }
} 