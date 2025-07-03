using System.ComponentModel.DataAnnotations;

namespace KunFarm.BLL.DTOs.Request
{
    public class UpdateUserRequest
    {
        [StringLength(100)]
        public string? DisplayName { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        public bool? IsActive { get; set; }
        
        public string? Role { get; set; }
    }
    
    public class UpdatePlayerStateRequest
    {
        public int? Money { get; set; }
        public float? PosX { get; set; }
        public float? PosY { get; set; }
        public float? PosZ { get; set; }
        public float? Health { get; set; }
        public float? Hunger { get; set; }
    }
} 