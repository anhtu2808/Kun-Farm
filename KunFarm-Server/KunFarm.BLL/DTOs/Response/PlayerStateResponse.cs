namespace KunFarm.BLL.DTOs.Response
{
    public class PlayerStateResponse
    {
        public int UserId { get; set; }
        public int Money { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float Health { get; set; }
        public float Hunger { get; set; }
        public DateTime LastSaved { get; set; }
    }
} 