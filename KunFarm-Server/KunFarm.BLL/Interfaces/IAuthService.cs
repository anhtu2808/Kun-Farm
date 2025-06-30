using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<bool> ValidateTokenAsync(string token);
        string GenerateToken(int userId, string username);
    }
} 