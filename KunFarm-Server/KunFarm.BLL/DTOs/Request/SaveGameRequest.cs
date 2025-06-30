using System.ComponentModel.DataAnnotations;

namespace KunFarm.BLL.DTOs.Request
{
    public class SaveGameRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be positive")]
        public int userId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Money must be non-negative")]
        public int money { get; set; }

        [Required]
        public float posX { get; set; }

        [Required]
        public float posY { get; set; }

        [Required]
        public float posZ { get; set; }
    }
} 