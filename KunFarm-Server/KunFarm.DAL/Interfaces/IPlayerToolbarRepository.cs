using KunFarm.DAL.Entities;

namespace KunFarm.DAL.Interfaces
{
    public interface IPlayerToolbarRepository
    {
        Task<PlayerToolbar?> GetByPlayerStateIdAsync(int playerStateId);
        Task<bool> SavePlayerToolbarAsync(int playerStateId, string toolsJson);
        Task<bool> CreatePlayerToolbarAsync(PlayerToolbar playerToolbar);
        Task<bool> DeletePlayerToolbarAsync(int playerStateId);
    }
} 