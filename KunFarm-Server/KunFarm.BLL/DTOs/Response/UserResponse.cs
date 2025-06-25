namespace KunFarm.BLL.DTOs.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }
} 