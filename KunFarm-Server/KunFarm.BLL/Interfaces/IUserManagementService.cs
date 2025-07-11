using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserListResponse>> GetAllUsersAsync();
        Task<UserDetailsResponse?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<bool> UpdatePlayerStateAsync(int userId, UpdatePlayerStateRequest request);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<List<UserListResponse>> SearchUsersAsync(string searchTerm);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
    }
} 