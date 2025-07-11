using KunFarm.DAL.Entities;

namespace KunFarm.DAL.Interfaces
{
    public interface IPlayerStateRepository
    {
        Task<PlayerState?> GetByUserIdAsync(int userId);
        Task<bool> SavePlayerStateAsync(int userId, int money, float posX, float posY, float posZ, float health, float hunger);
        Task<bool> UpdatePlayerPositionAsync(int userId, float posX, float posY, float posZ);
        Task<bool> UpdatePlayerMoneyAsync(int userId, int money);
        Task<bool> CreatePlayerStateAsync(PlayerState playerState);
        Task<bool> DeletePlayerStateAsync(int userId);
    }
} 