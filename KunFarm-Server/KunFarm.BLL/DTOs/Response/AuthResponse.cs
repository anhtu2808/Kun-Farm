namespace KunFarm.BLL.DTOs.Response
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
    }
} 