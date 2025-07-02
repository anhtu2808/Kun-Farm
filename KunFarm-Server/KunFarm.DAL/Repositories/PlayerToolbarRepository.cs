using KunFarm.DAL.Data;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KunFarm.DAL.Repositories
{
    public class PlayerToolbarRepository : IPlayerToolbarRepository
    {
        private readonly KunFarmDbContext _context;

        public PlayerToolbarRepository(KunFarmDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerToolbar?> GetByPlayerStateIdAsync(int playerStateId)
        {
            return await _context.PlayerToolbars
                .FirstOrDefaultAsync(pt => pt.PlayerStateId == playerStateId);
        }

        public async Task<bool> SavePlayerToolbarAsync(int playerStateId, string toolsJson)
        {
            try
            {
                var existingToolbar = await GetByPlayerStateIdAsync(playerStateId);
                
                if (existingToolbar != null)
                {
                    // Update existing toolbar
                    existingToolbar.ToolsJson = toolsJson;
                    existingToolbar.LastSaved = DateTime.UtcNow;
                    
                    _context.PlayerToolbars.Update(existingToolbar);
                }
                else
                {
                    // Create new toolbar
                    var newToolbar = new PlayerToolbar
                    {
                        PlayerStateId = playerStateId,
                        ToolsJson = toolsJson,
                        LastSaved = DateTime.UtcNow
                    };
                    
                    await _context.PlayerToolbars.AddAsync(newToolbar);
                }
                
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreatePlayerToolbarAsync(PlayerToolbar playerToolbar)
        {
            try
            {
                await _context.PlayerToolbars.AddAsync(playerToolbar);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePlayerToolbarAsync(int playerStateId)
        {
            try
            {
                var toolbar = await GetByPlayerStateIdAsync(playerStateId);
                if (toolbar == null) return false;
                
                _context.PlayerToolbars.Remove(toolbar);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 