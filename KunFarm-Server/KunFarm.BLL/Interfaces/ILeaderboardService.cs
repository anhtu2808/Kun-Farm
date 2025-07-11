using KunFarm.BLL.DTOs.Response;

namespace KunFarm.BLL.Interfaces
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponse> GetMoneyLeaderboardAsync(int top = 100);
        Task<int?> GetPlayerMoneyRankAsync(int userId);
    }
} 