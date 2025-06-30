using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace KunFarm.BLL.Services
{
    public class GameService : IGameService
    {
        private readonly IPlayerStateRepository _playerStateRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IPlayerStateRepository playerStateRepository,
            IUserRepository userRepository,
            ILogger<GameService> logger)
        {
            _playerStateRepository = playerStateRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<PlayerStateResponse?> LoadGameAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Loading game state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return null;
                }

                // Get player state
                var playerState = await _playerStateRepository.GetByUserIdAsync(userId);
                
                if (playerState == null)
                {
                    _logger.LogInformation("No save data found for user: {UserId}, creating default state", userId);
                    
                    // Create default player state
                    var defaultState = new PlayerState
                    {
                        UserId = userId,
                        Money = 0,
                        PosX = 0f,
                        PosY = 0f,
                        PosZ = 0f,
                        LastSaved = DateTime.UtcNow
                    };

                    await _playerStateRepository.CreatePlayerStateAsync(defaultState);
                    return new PlayerStateResponse
                    {
                        UserId = defaultState.UserId,
                        Money = defaultState.Money,
                        PosX = defaultState.PosX,
                        PosY = defaultState.PosY,
                        PosZ = defaultState.PosZ,
                        LastSaved = defaultState.LastSaved
                    };
                }

                _logger.LogInformation("Game state loaded successfully for user: {UserId}", userId);
                return new PlayerStateResponse
                {
                    UserId = playerState.UserId,
                    Money = playerState.Money,
                    PosX = playerState.PosX,
                    PosY = playerState.PosY,
                    PosZ = playerState.PosZ,
                    LastSaved = playerState.LastSaved
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading game state for user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> SaveGameAsync(int userId, SaveGameRequest request)
        {
            try
            {
                _logger.LogInformation("Saving game state for user: {UserId}", userId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return false;
                }

                // Save player state
                var success = await _playerStateRepository.SavePlayerStateAsync(
                    userId, 
                    request.money, 
                    request.posX, 
                    request.posY, 
                    request.posZ);

                if (success)
                {
                    _logger.LogInformation("Game state saved successfully for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to save game state for user: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game state for user: {UserId}", userId);
                return false;
            }
        }
    }
} 