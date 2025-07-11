using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface IGameService
    {
        Task<PlayerStateResponse?> LoadGameAsync(int userId);
        Task<bool> SaveGameAsync(int userId, SaveGameRequest request);
        Task<FarmStateResponse?> LoadFarmStateAsync(int userId);
        Task<bool> SaveFarmStateAsync(int userId, SaveFarmStateRequest request);
        Task<ToolbarResponse?> LoadToolbarAsync(int userId);
        Task<bool> SaveToolbarAsync(int userId, SaveToolbarRequest request);
    }
} 