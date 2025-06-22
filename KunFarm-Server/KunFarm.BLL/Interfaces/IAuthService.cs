using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<bool> ValidateTokenAsync(string token);
        string GenerateToken(int userId, string username);
    }
} 