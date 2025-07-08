namespace KunFarm.BLL.DTOs.Response
{
    public class FarmStateResponse
    {
        public int UserId { get; set; }
        public List<TileStateResponseData> TileStates { get; set; } = new List<TileStateResponseData>();
        public List<PlantResponseData> Plants { get; set; } = new List<PlantResponseData>();
        public DateTime LastSaved { get; set; }
        
        // Di chuyển từ PlayerStateResponse
        public string ChickensStateJson { get; set; } = "[]";
        public string EggsStateJson { get; set; } = "[]";
    }

    public class TileStateResponseData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int State { get; set; }
    }

    public class PlantResponseData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string CropType { get; set; } = string.Empty;
        public int CurrentStage { get; set; }
        public float Timer { get; set; }
        public bool IsMature { get; set; }
    }
} 