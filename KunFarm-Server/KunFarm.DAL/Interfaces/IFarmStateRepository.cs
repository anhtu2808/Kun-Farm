using KunFarm.DAL.Entities;

namespace KunFarm.DAL.Interfaces
{
    public interface IFarmStateRepository
    {
        Task<FarmState?> GetByUserIdAsync(int userId);
        Task<bool> SaveFarmStateAsync(int userId, string tileStatesJson, string plantsJson, string? chickensStateJson = null, string? eggsStateJson = null);
        Task<bool> CreateFarmStateAsync(FarmState farmState);
        Task<bool> DeleteFarmStateAsync(int userId);
    }
} 