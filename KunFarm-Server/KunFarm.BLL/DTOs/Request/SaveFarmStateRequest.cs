using System.ComponentModel.DataAnnotations;

namespace KunFarm.BLL.DTOs.Request
{
    public class SaveFarmStateRequest
    {
        [Required]
        public int userId { get; set; }
        
        public List<TileStateData> tileStates { get; set; } = new List<TileStateData>();
        
        public List<PlantData> plants { get; set; } = new List<PlantData>();
    }

    public class TileStateData
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int state { get; set; } // 0=Undug, 1=Dug, 2=Planted, 3=Harvested
    }

    public class PlantData
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public string cropType { get; set; } = string.Empty; // "Wheat", "Grapes", "AppleTree"
        public int currentStage { get; set; }
        public float timer { get; set; }
        public bool isMature { get; set; }
    }
} 