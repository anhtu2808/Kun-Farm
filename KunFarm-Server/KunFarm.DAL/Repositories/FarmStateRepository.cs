using KunFarm.DAL.Data;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KunFarm.DAL.Repositories
{
    public class FarmStateRepository : IFarmStateRepository
    {
        private readonly KunFarmDbContext _context;

        public FarmStateRepository(KunFarmDbContext context)
        {
            _context = context;
        }

        public async Task<FarmState?> GetByUserIdAsync(int userId)
        {
            return await _context.FarmStates
                .Include(fs => fs.User)
                .FirstOrDefaultAsync(fs => fs.UserId == userId);
        }

        public async Task<bool> SaveFarmStateAsync(int userId, string tileStatesJson, string plantsJson)
        {
            try
            {
                var existingState = await GetByUserIdAsync(userId);
                
                if (existingState != null)
                {
                    // Update existing state
                    existingState.TileStatesJson = tileStatesJson;
                    existingState.PlantsJson = plantsJson;
                    existingState.LastSaved = DateTime.UtcNow;
                    
                    _context.FarmStates.Update(existingState);
                }
                else
                {
                    // Create new state
                    var newState = new FarmState
                    {
                        UserId = userId,
                        TileStatesJson = tileStatesJson,
                        PlantsJson = plantsJson,
                        LastSaved = DateTime.UtcNow
                    };
                    
                    await _context.FarmStates.AddAsync(newState);
                }
                
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateFarmStateAsync(FarmState farmState)
        {
            try
            {
                await _context.FarmStates.AddAsync(farmState);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFarmStateAsync(int userId)
        {
            try
            {
                var state = await GetByUserIdAsync(userId);
                if (state == null) return false;
                
                _context.FarmStates.Remove(state);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 