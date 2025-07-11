using KunFarm.DAL.Data;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KunFarm.DAL.Repositories
{
    public class PlayerStateRepository : IPlayerStateRepository
    {
        private readonly KunFarmDbContext _context;

        public PlayerStateRepository(KunFarmDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerState?> GetByUserIdAsync(int userId)
        {
            return await _context.PlayerStates
                .Include(ps => ps.User)
                .FirstOrDefaultAsync(ps => ps.UserId == userId);
        }

        public async Task<bool> SavePlayerStateAsync(int userId, int money, float posX, float posY, float posZ, float health, float hunger)
        {
            try
            {
                var existingState = await GetByUserIdAsync(userId);
                
                if (existingState != null)
                {
                    // Update existing state
                    existingState.Money = money;
                    existingState.PosX = posX;
                    existingState.PosY = posY;
                    existingState.PosZ = posZ;
                    existingState.Health = health;
                    existingState.Hunger = hunger;
                    existingState.LastSaved = DateTime.UtcNow;
                    
                    _context.PlayerStates.Update(existingState);
                }
                else
                {
                    // Create new state
                    var newState = new PlayerState
                    {
                        UserId = userId,
                        Money = money,
                        PosX = posX,
                        PosY = posY,
                        PosZ = posZ,
                        Health = health,
                        Hunger = hunger,
                        LastSaved = DateTime.UtcNow
                    };
                    
                    await _context.PlayerStates.AddAsync(newState);
                }
                
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePlayerPositionAsync(int userId, float posX, float posY, float posZ)
        {
            try
            {
                var state = await GetByUserIdAsync(userId);
                if (state == null) return false;
                
                state.PosX = posX;
                state.PosY = posY;
                state.PosZ = posZ;
                state.LastSaved = DateTime.UtcNow;
                
                _context.PlayerStates.Update(state);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePlayerMoneyAsync(int userId, int money)
        {
            try
            {
                var state = await GetByUserIdAsync(userId);
                if (state == null) return false;
                
                state.Money = money;
                state.LastSaved = DateTime.UtcNow;
                
                _context.PlayerStates.Update(state);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreatePlayerStateAsync(PlayerState playerState)
        {
            try
            {
                await _context.PlayerStates.AddAsync(playerState);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePlayerStateAsync(int userId)
        {
            try
            {
                var state = await GetByUserIdAsync(userId);
                if (state == null) return false;
                
                _context.PlayerStates.Remove(state);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePlayerAsync(PlayerState player)
		{
			try
			{
				_context.PlayerStates.Update(player);
				return await _context.SaveChangesAsync() > 0;
			}
			catch
			{
				return false;
			}
		}

	}
} 