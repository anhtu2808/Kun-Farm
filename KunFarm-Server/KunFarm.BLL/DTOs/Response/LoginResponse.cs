namespace KunFarm.BLL.DTOs.Response
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public string? Token { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public decimal Coins { get; set; }
        public decimal Gems { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
} 