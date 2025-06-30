using System.ComponentModel.DataAnnotations;

namespace KunFarm.DAL.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public Role Role { get; set; } = Role.PLAYER;
        
        // Navigation property
        public PlayerState? PlayerState { get; set; }
    }
} 