using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KunFarm.DAL.Entities
{
    public class PlayerToolbar
    {
        [Key]
        [ForeignKey("PlayerState")]
        public int PlayerStateId { get; set; }
        
        [Required]
        [Column(TypeName = "json")]
        public string ToolsJson { get; set; } = "[]";
        
        public DateTime LastSaved { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public PlayerState PlayerState { get; set; } = null!;
    }
} 