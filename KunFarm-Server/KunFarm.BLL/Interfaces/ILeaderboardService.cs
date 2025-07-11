using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponse> GetMoneyLeaderboardAsync(int top = 100);
        Task<LeaderboardResponse> GetHealthLeaderboardAsync(int top = 100);
        Task<LeaderboardResponse> GetHungerLeaderboardAsync(int top = 100);
        Task<int?> GetPlayerMoneyRankAsync(int userId);
        Task<int?> GetPlayerHealthRankAsync(int userId);
        Task<int?> GetPlayerHungerRankAsync(int userId);
    }
} 