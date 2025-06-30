using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KunFarm.DAL.Entities
{
    public class FarmState
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [Required]
        [Column(TypeName = "json")]
        public string TileStatesJson { get; set; } = "[]";
        
        [Required]
        [Column(TypeName = "json")]
        public string PlantsJson { get; set; } = "[]";
        
        public DateTime LastSaved { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public User User { get; set; } = null!;
    }
} 