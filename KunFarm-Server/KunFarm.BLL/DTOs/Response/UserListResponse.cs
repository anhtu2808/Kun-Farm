namespace KunFarm.BLL.DTOs.Response
{
    public class UserListResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Money { get; set; }
        public float Health { get; set; }
        public float Hunger { get; set; }
        public DateTime LastSaved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 