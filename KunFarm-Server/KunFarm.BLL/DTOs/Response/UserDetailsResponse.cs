namespace KunFarm.BLL.DTOs.Response
{
    public class UserDetailsResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        
        // Player State Information
        public PlayerStateInfo? PlayerState { get; set; }
    }
    
    public class PlayerStateInfo
    {
        public int Money { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float Health { get; set; }
        public float Hunger { get; set; }
        public DateTime LastSaved { get; set; }
    }
} 